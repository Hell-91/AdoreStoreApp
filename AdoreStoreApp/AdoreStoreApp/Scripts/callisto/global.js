(function($){
    $(document).ready(function(){		//when DOM is ready
        soulage.init();
    });
})(jQuery);

var soulage = {
	activeQuickshop: null,
	init: function() {
		this.initScrollTop();
		this.initQuickShop();
		this.initHomepageSlider();
		this.initSingleProductImageSlider();
		this.initSingleProductDescriptionTabs();
		this.initFormElements();
		this.initNavigationSelector();
		this.initCart();
		this.initAccountLogin();
		this.initAddressManage();
		this.initProductLightbox();
	},
	initScrollTop: function() {
		$('.back-to-the-top').click(function () {
			$('body,html').animate({
				scrollTop: 0
			}, 400);
			return false;
		});
	},
	initQuickShop: function() {		
		$('.item-block-1 .button-1, .item-block-2 .button-1').click(function(){
			soulage.adjustQuickShopPopupPosition();
			$('.lightbox').animate({'opacity':'0.35'}, 0, 'linear');
			$('.quick-shop').animate({'opacity':'1'}, 0, 'linear');
			$('.lightbox, .quick-shop').css('display', 'block');
			return false;
		});
		$('.lightbox, .close').on('click', function(){
			soulage.closeQuickShopPopup();
			return false;
		});
		$(window).resize(function(){
			soulage.adjustQuickShopPopupPosition();
		});
	},
	adjustQuickShopPopupPosition: function() {
		$('.quick-shop').css({
			position: 'fixed',
			left: ($(window).width() - $('.quick-shop').outerWidth())/2,
			top: ($(window).height() - $('.quick-shop').outerHeight())/2
		});	
	},
	closeQuickShopPopup: function() {
		$('.quick-shop').fadeOut(350);			//close popup
		$('.lightbox').hide();
	},
	initHomepageSlider: function() {
		$('#hompage-slider_content').cycle({
			fx: 'scrollHorz',
			speed: '600',
			timeout: '5000',
			prev:   '.previous', 
			next:   '.next',
			pager: '#pager',
			activePagerClass: "active",
			easing: 'swing',
			slideResize: 0,
			containerResize: 1,
			slideExpr: '.item',
			pagerAnchorBuilder: function(idx, slide) {  return ''; }
		});

		$('.navigation .bullet').click(function(){
			var index = $('.navigation #pager a').index($(this));
			$('#hompage-slider_content').cycle(index);
			return false;
		});
	},
	initSingleProductImageSlider: function() {						
		$('#single-product-slider').cycle({
			fx: 'scrollHorz',
			speed: '600',
			timeout: 0,
			easing: 'swing',
			slideResize: 0,
			containerResize: 1,
			slideExpr: '.image'
		});

		var navBtns = $('.main-item-wrapper .image-wrapper-4');
		navBtns.click(function(){
			var index = navBtns.index($(this));
			navBtns.removeClass('active');
			$(this).addClass('active');
			$('#single-product-slider').cycle(index);
			return false;
		});
	},
	initSingleProductDescriptionTabs: function() {						
				
		$('#description_slider').cycle({
			fx: 'scrollHorz',
			speed: '600',
			timeout: 0,
			easing: 'swing'
		});

		var navBtns = $('.button-navigation a');
		navBtns.click(function(){
			var index = navBtns.index($(this));
			navBtns.removeClass('active');
			$(this).addClass('active');
			$('#description_slider').cycle(index);
			return false;
		});
	},
	initFormElements: function() {
		$("select").uniform();
	},
	initNavigationSelector: function() {
		var selector = $('.navigationSelector');
		if(selector.length > 0)
		{
			selector.change(function(){
				window.location = $('option:selected', $(this)).attr('value');
			});
		}
	},
	initCart: function() {	
		// + 1 quantity
		$('.cart-items .quantity .plus').click(function(){
			var quantity = $(this).siblings('input[type=text]');
			var newQuantity = parseInt(quantity.val()) + 1;
			quantity.val(newQuantity);
			return false;
		});
		
		// - 1 quantity
		$('.cart-items .quantity .minus').click(function(){
			var quantity = $(this).siblings('input[type=text]');
			var oldQuantity = parseInt(quantity.val());
			if(oldQuantity > 1)
			{
				var newQuantity = oldQuantity - 1;
				quantity.val(newQuantity);
			}
			return false;
		});
		
		//allow only numbers to be entered in quantity box
		$('.cart-items .quantity input[type=text]').keydown(function(event) {
	        // Allow: backspace, delete, tab and escape
	        if ( event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27
		 		|| 
	            (event.keyCode == 65 && event.ctrlKey === true) 
				|| 
	            (event.keyCode >= 35 && event.keyCode <= 39)) {
	                 return;
	        }

	        // Ensure that it is a number and stop the keypress
	        if ((event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105 )) {
	            event.preventDefault(); 
	        }   
		});
	},
	initAccountLogin: function() {
		if (window.location.hash == '#recover') {
			$('#login_form').hide();
			$('#password_recovery').show();
		}
		
		$('#forgot_password').click(function() {
			$('#login_form').fadeOut(200, function(){
				$('#password_recovery').fadeIn(200);
			});
			return false;
		});
	
		$('#login').click(function() {
			$('#password_recovery').fadeOut(200, function(){
				$('#login_form').fadeIn(200);
			});
			return false;
		});
	
		$('#login_submit').click(function() {
			$('#customer_login').submit();
			return false;
		});
	
		$('#recover_submit').click(function() {
			$(this).parents('form').submit();
			return false;
		});
	},
	initAddressManage: function() {			
		$('.edit-address-btn').click(function(){
			var editForm = 'edit_' + $(this).parents('.row').attr('id');				
			$('#' + editForm ).slideDown(300);
			return false;
		});
		
		$('.address-edit-form-cancel').click(function(){
			$(this).parents('.address-edit-form').slideUp(300);
			return false;
		});
	},
	initProductLightbox: function() {
		$('.lightbox-launcher').click(function(){
			
			var elem = $(this);
			var imgSrc = elem.attr('href');
			$('.lightbox').show();
			
			var productImage = new Image();
			productImage.onload = function() {
				$('.quick-shop').html(productImage);
				$('.quick-shop').append('<a href="#" class="close"></a>');
				soulage.adjustQuickShopPopupPosition();
				$('.quick-shop').fadeIn(350, function(){
					soulage.adjustQuickShopPopupPosition();
				});
			};
			productImage.src = imgSrc;
			
			return false;
		});
		
		$('.lightbox, .close, .quick-shop').on('click', function(){
			soulage.closeQuickShopPopup();
			return false;
		});

		$(window).resize(function(){
			soulage.adjustQuickShopPopupPosition();
		});
	}
}