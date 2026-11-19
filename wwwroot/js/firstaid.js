(function () {
  var form = document.getElementById('medical');
  var msg = document.getElementById('medMsg');
  var saved = HC.read('hc.medical', {});
  Object.keys(saved).forEach(function (k) { if (form.elements[k]) form.elements[k].value = saved[k]; });
  form.addEventListener('submit', function (ev) {
    ev.preventDefault();
    var data = {};
    new FormData(form).forEach(function (v, k) { data[k] = String(v).slice(0, 200); });
    HC.write('hc.medical', data);
    HC.log('Medical details saved on this device.');
    msg.textContent = 'Saved on this device only.';
  });
  // Highlight the card linked from an alert.
  if (location.hash) {
    var el = document.querySelector(location.hash);
    if (el) el.classList.add('target');
  }
})();
