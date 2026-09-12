// Yangi Alif — admin panel skripti.
// Alohida faylda: sayt CSP qoidasi (script-src 'self') ichki <script> ni bloklaydi.

(function () {
  var gate = document.getElementById('gate');
  var dash = document.getElementById('dash');
  var pwInput = document.getElementById('pwInput');
  var loginBtn = document.getElementById('loginBtn');
  var loginError = document.getElementById('loginError');
  var logoutBtn = document.getElementById('logoutBtn');

  function showError(msg) {
    loginError.textContent = msg;
    loginError.classList.add('show');
  }

  function renderChart(days) {
    var chart = document.getElementById('chart');
    chart.innerHTML = '';
    var max = 1;
    days.forEach(function (d) { if (d.total > max) max = d.total; });
    days.forEach(function (d) {
      var col = document.createElement('div');
      col.className = 'col';
      var stack = document.createElement('div');
      stack.className = 'bar-stack';
      stack.style.height = Math.max(4, Math.round((d.total / max) * 130)) + 'px';
      stack.title = d.date + ': ' + d.exe + ' .exe, ' + d.zip + ' .zip';
      if (d.exe > 0) {
        var exeBar = document.createElement('div');
        exeBar.className = 'bar-exe';
        exeBar.style.height = (d.total > 0 ? (d.exe / d.total) * 100 : 0) + '%';
        stack.appendChild(exeBar);
      }
      if (d.zip > 0) {
        var zipBar = document.createElement('div');
        zipBar.className = 'bar-zip';
        zipBar.style.height = (d.total > 0 ? (d.zip / d.total) * 100 : 0) + '%';
        stack.appendChild(zipBar);
      }
      col.appendChild(stack);
      var label = document.createElement('div');
      label.className = 'day-label';
      label.textContent = d.date.slice(5); // MM-DD
      col.appendChild(label);
      chart.appendChild(col);
    });
  }

  function loadStats(password) {
    loginBtn.disabled = true;
    loginError.classList.remove('show');
    fetch('/api/stats', { headers: { 'Authorization': 'Bearer ' + password }, cache: 'no-store' })
      .then(function (r) { return r.json().then(function (body) { return { status: r.status, body: body }; }); })
      .then(function (res) {
        loginBtn.disabled = false;
        if (res.status === 401) { showError('Parol noto\'g\'ri.'); return; }
        if (res.status === 429) { showError('Juda ko\'p noto\'g\'ri urinish. 15 daqiqadan keyin qayta urining.'); return; }
        if (!res.body.ok) { showError(res.body.error || 'Xatolik yuz berdi.'); return; }

        try { sessionStorage.setItem('yangialif-admin-pw', password); } catch (e) { }
        gate.classList.add('hidden');
        dash.classList.add('show');

        document.getElementById('statTotal').textContent = res.body.total;
        document.getElementById('statExe').textContent = res.body.totalExe;
        document.getElementById('statZip').textContent = res.body.totalZip;
        renderChart(res.body.days);
      })
      .catch(function () {
        loginBtn.disabled = false;
        showError('Serverga ulanib bo\'lmadi. Sayt Cloudflare Pages\'ga joylashtirilganmi va DOWNLOADS/ADMIN_PASSWORD sozlanganmi, tekshiring.');
      });
  }

  loginBtn.addEventListener('click', function () {
    if (pwInput.value) loadStats(pwInput.value);
  });
  pwInput.addEventListener('keydown', function (e) {
    if (e.key === 'Enter') loginBtn.click();
  });
  logoutBtn.addEventListener('click', function () {
    try { sessionStorage.removeItem('yangialif-admin-pw'); } catch (e) { }
    dash.classList.remove('show');
    gate.classList.remove('hidden');
    pwInput.value = '';
    pwInput.focus();
  });

  // Shu tabda avval kirilgan bo'lsa, qayta so'ramaymiz.
  var saved = null;
  try { saved = sessionStorage.getItem('yangialif-admin-pw'); } catch (e) { }
  if (saved) { loadStats(saved); } else { pwInput.focus(); }
})();
