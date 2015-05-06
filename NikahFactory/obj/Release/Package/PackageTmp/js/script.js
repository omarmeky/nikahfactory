// Roller JS

$(document).ready(function() {

  // Bootstrap Scrollspy
  $('body').scrollspy({offset: 100});

  // Setup Fitvids Container
  $(".video-container").fitVids();

  // Gallery Popup
  $('.image-popup-vertical-fit').magnificPopup({
    type: 'image',
    closeOnContentClick: true,
    mainClass: 'my-mfp-zoom-in',
    image: {
      verticalFit: true
    }
  });

  // Set Header to Absolute on Input Focus
  if (Modernizr.touch) {
    var $body = $('body');
    var $inputs = $('input');
    $inputs.on('focus', function(e) {
      $body.addClass('fixed');
    });
    $inputs.on('blur', function(e) {
      $body.removeClass('fixed');
    });
  }

});