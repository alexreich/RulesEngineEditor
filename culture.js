window.blazorCulture = {
  get: function () {
    return window.localStorage['BlazorCulture'];
  },
  set: function (value) {
    window.localStorage['BlazorCulture'] = value;
  }
};

window.setCultureCookie = function (culture) {
  var cookieValue = 'c=' + culture + '|uic=' + culture;
  var secure = (location.protocol === 'https:') ? '; secure' : '';
  document.cookie = '.AspNetCore.Culture=' + cookieValue + '; path=/; samesite=lax' + secure;
};