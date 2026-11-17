var global = {
    mobileClient: false,
    savePermit: true,
    usd: 0,
    eur: 0
};

/**
 * Oauth2
 */

/**
 * OAuth2 authorization code + PKCE against Spring Authorization Server (uaa).
 * The bundled form posts the credentials to uaa/login, then the browser session
 * is exchanged for a code (redirect_uri = this page) and the code for a token.
 */

var OAUTH_CLIENT_ID = 'browser';

function oauthRedirectUri() {
	return location.protocol + '//' + location.host + '/';
}

function base64Url(bytes) {
	var binary = '';
	for (var i = 0; i < bytes.length; i++) {
		binary += String.fromCharCode(bytes[i]);
	}
	return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

function randomBase64Url(size) {
	return base64Url(window.crypto.getRandomValues(new Uint8Array(size)));
}

// SHA-256 of an ASCII string. crypto.subtle is async and missing on plain-http hosts.
function sha256(ascii) {
	var K = [], H = [], i, j;
	var isPrime = function (n) { for (var f = 2; f * f <= n; f++) { if (n % f === 0) return false; } return true; };
	var frac = function (x) { return ((x - Math.floor(x)) * 4294967296) | 0; };
	for (var c = 2, n = 0; n < 64; c++) {
		if (isPrime(c)) {
			if (n < 8) H[n] = frac(Math.pow(c, 1 / 2));
			K[n++] = frac(Math.pow(c, 1 / 3));
		}
	}
	var bytes = [];
	for (i = 0; i < ascii.length; i++) bytes.push(ascii.charCodeAt(i) & 0xff);
	var bitLength = bytes.length * 8;
	bytes.push(0x80);
	while (bytes.length % 64 !== 56) bytes.push(0);
	for (i = 7; i >= 0; i--) bytes.push(i >= 4 ? 0 : (bitLength >>> (i * 8)) & 0xff);

	var rotr = function (x, r) { return (x >>> r) | (x << (32 - r)); };
	var w = new Array(64);
	for (var offset = 0; offset < bytes.length; offset += 64) {
		for (i = 0; i < 16; i++) {
			j = offset + 4 * i;
			w[i] = (bytes[j] << 24) | (bytes[j + 1] << 16) | (bytes[j + 2] << 8) | bytes[j + 3];
		}
		for (i = 16; i < 64; i++) {
			var s0 = rotr(w[i - 15], 7) ^ rotr(w[i - 15], 18) ^ (w[i - 15] >>> 3);
			var s1 = rotr(w[i - 2], 17) ^ rotr(w[i - 2], 19) ^ (w[i - 2] >>> 10);
			w[i] = (w[i - 16] + s0 + w[i - 7] + s1) | 0;
		}
		var a = H[0], b = H[1], cc = H[2], d = H[3], e = H[4], f = H[5], g = H[6], h = H[7];
		for (i = 0; i < 64; i++) {
			var t1 = (h + (rotr(e, 6) ^ rotr(e, 11) ^ rotr(e, 25)) + ((e & f) ^ (~e & g)) + K[i] + w[i]) | 0;
			var t2 = ((rotr(a, 2) ^ rotr(a, 13) ^ rotr(a, 22)) + ((a & b) ^ (a & cc) ^ (b & cc))) | 0;
			h = g; g = f; f = e; e = (d + t1) | 0; d = cc; cc = b; b = a; a = (t1 + t2) | 0;
		}
		H[0] = (H[0] + a) | 0; H[1] = (H[1] + b) | 0; H[2] = (H[2] + cc) | 0; H[3] = (H[3] + d) | 0;
		H[4] = (H[4] + e) | 0; H[5] = (H[5] + f) | 0; H[6] = (H[6] + g) | 0; H[7] = (H[7] + h) | 0;
	}
	var out = [];
	for (i = 0; i < 8; i++) out.push((H[i] >>> 24) & 0xff, (H[i] >>> 16) & 0xff, (H[i] >>> 8) & 0xff, H[i] & 0xff);
	return out;
}

function syncRequest(method, url, formData) {
	var xhr = new XMLHttpRequest();
	xhr.open(method, url, false);
	if (formData) {
		xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
	}
	xhr.send(formData ? $.param(formData) : null);
	return xhr;
}

function requestOauthToken(username, password) {
	try {
		var login = syncRequest('POST', 'uaa/login', {username: username, password: password});
		if (login.status === 0 || /[?&]error/.test(login.responseURL)) {
			throw new Error('login failed');
		}

		var verifier = randomBase64Url(32);
		var state = randomBase64Url(16);
		var authorize = syncRequest('GET', 'uaa/oauth2/authorize?' + $.param({
			response_type: 'code',
			client_id: OAUTH_CLIENT_ID,
			scope: 'ui',
			redirect_uri: oauthRedirectUri(),
			state: state,
			code_challenge: base64Url(sha256(verifier)),
			code_challenge_method: 'S256'
		}));
		var params = new URL(authorize.responseURL).searchParams;
		if (params.get('state') !== state || !params.get('code')) {
			throw new Error('no authorization code');
		}

		var token = syncRequest('POST', 'uaa/oauth2/token', {
			grant_type: 'authorization_code',
			client_id: OAUTH_CLIENT_ID,
			code: params.get('code'),
			redirect_uri: oauthRedirectUri(),
			code_verifier: verifier
		});
		if (token.status !== 200) {
			throw new Error('token exchange failed');
		}
		localStorage.setItem('token', JSON.parse(token.responseText).access_token);
		return true;
	} catch (e) {
		removeOauthTokenFromStorage();
		return false;
	}
}

function endAuthServerSession() {
	try {
		syncRequest('POST', 'uaa/logout', {});
	} catch (e) {
		// the access token is dropped either way
	}
}

function getOauthTokenFromStorage() {
	return localStorage.getItem('token');
}

function removeOauthTokenFromStorage() {
    return localStorage.removeItem('token');
}

/**
 * Current account
 */

function getCurrentAccount() {

	var token = getOauthTokenFromStorage();
	var account = null;

	if (token) {
		$.ajax({
			url: 'accounts/current',
			datatype: 'json',
			type: 'get',
			headers: {'Authorization': 'Bearer ' + token},
			async: false,
			success: function (data) {
				account = data;
			},
			error: function () {
				removeOauthTokenFromStorage();
			}
		});
	}

	return account;
}

$(window).load(function(){

	if(/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ) {
		FastClick.attach(document.body);
        global.mobileClient = true;
	}

    $.getJSON("https://open.er-api.com/v6/latest/RUB", function( data ) {
        global.eur = 1 / data.rates.EUR;
        global.usd = 1 / data.rates.USD;
    });

	var account = getCurrentAccount();

	if (account) {
		showGreetingPage(account);
	} else {
		showLoginForm();
	}
});

function showGreetingPage(account) {
    initAccount(account);
	var userAvatar = $("<img />").attr("src","images/userpic.jpg");
	$(userAvatar).load(function() {
		setTimeout(initGreetingPage, 500);
	});
}

function showLoginForm() {
	$("#loginpage").show();
	$("#frontloginform").focus();
	setTimeout(initialShaking, 700);
}