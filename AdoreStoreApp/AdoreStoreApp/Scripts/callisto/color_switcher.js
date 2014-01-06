function init()
{
	$(document).ready(function() {
	
		var color = get_color();
		createCookie('callisto_color', color, 30);
		var checkout = false;
		if($('body').hasClass('checkout'))
		{
			checkout = true;
		}
		
		if(color == 'dark')
		{
			if ( $.browser.msie ) {
			  	
				document.createStyleSheet('css/dark.css');
			  	
				if(parseInt($.browser.version, 10) < 9)
			   	{
			   		if(checkout)
			   		{
			   			document.createStyleSheet('css/dark-checkout.css');
			   			document.createStyleSheet('css/ie-checkout-dark.css');		   		
			   		}
			   		else
			   		{
			   			document.createStyleSheet('css/ie-dark.css');
			   		}
			   	}
			}
			else
			{
				if(checkout)
			   	{
					$('head').append('<link rel="stylesheet" href="css/dark-checkout.css" type="text/css" />');
				}

				$('head').append('<link rel="stylesheet" href="css/dark.css" type="text/css" />');

			}
			
		
				$('.logo a img').attr('src', 'images/logo-soulage-2.png');
		}
	});
}


function get_color() {
	if(readCookie('callisto_color') == 'dark')
	{
		return 'dark';
	}
	else
	{
		return 'light';
	}
}

function toggle_color()
{
	var color = readCookie('callisto_color');
	if(color == 'light')
	{
		createCookie('callisto_color', 'dark', 30);
		console.log('toggle - ' + color);
	}
	else if(color == 'dark')
	{
		createCookie('callisto_color', 'light', 30);
	}
	location.reload(true);
}

function createCookie(name,value,days) {
	if (days) {
		var date = new Date();
		date.setTime(date.getTime()+(days*24*60*60*1000));
		var expires = "; expires="+date.toGMTString();
	}
	else var expires = "";
	document.cookie = name+"="+value+expires+"; path=/";
}

function readCookie(name) {
	var nameEQ = name + "=";
	var ca = document.cookie.split(';');
	for(var i=0;i < ca.length;i++) {
		var c = ca[i];
		while (c.charAt(0)==' ') c = c.substring(1,c.length);
		if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length,c.length);
	}
	return null;
}

function eraseCookie(name) {
	createCookie(name,"",-1);
}

init();