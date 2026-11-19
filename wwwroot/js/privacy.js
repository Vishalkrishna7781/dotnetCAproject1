(function () {
  var list = document.getElementById('privacyLog');
  function render() {
    var entries = HC.read(HC.LOG_KEY, []);
    list.innerHTML = '';
    if (!entries.length) { var li = document.createElement('li'); li.className = 'muted'; li.textContent = 'No entries yet.'; list.appendChild(li); return; }
    entries.forEach(function (e) {
      var li = document.createElement('li');
      var t = document.createElement('time'); t.textContent = new Date(e.t).toLocaleString();
      var s = document.createElement('span'); s.textContent = e.text;
      li.appendChild(t); li.appendChild(s); list.appendChild(li);
    });
  }
  document.getElementById('export').addEventListener('click', function () {
    var data = { medical: HC.read('hc.medical', {}), privacyLog: HC.read(HC.LOG_KEY, []) };
    var blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    var a = document.createElement('a');
    a.href = URL.createObjectURL(blob); a.download = 'my-health-data.json'; a.click();
    URL.revokeObjectURL(a.href);
    HC.log('Data exported by user.');
    render();
  });
  document.getElementById('wipe').addEventListener('click', function () {
    if (!confirm('Permanently delete all data stored by this app on this device?')) return;
    try { localStorage.removeItem('hc.medical'); localStorage.removeItem(HC.LOG_KEY); } catch (e) {}
    render();
  });
  render();
})();
