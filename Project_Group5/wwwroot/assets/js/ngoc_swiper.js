//xử lí nút bấm slide
document.addEventListener('DOMContentLoaded', function () {
  let btn_odd = 1;
  let btn_even = 2;
  let swiper_count = 1;
  let string_id = 'btn_'
  let string_swiper_class = '#swiper_position_'
  while (btn_odd <= 1000) {
      let string_left_btn = string_id + btn_odd.toString()
      let string_right_btn = string_id + btn_even.toString()
      let string_swiper_id = string_swiper_class + swiper_count.toString()


      btn_odd += 2
      btn_even += 2
      let mySwiperxyz = null;
      if (swiper_count == 1) {
          mySwiperxyz = new Swiper(string_swiper_id, {
              slidesPerView: 3,
              spaceBetween: 40,
              freeMode: true,
              autoplay: {
                delay: 4000,
                disableOnInteraction: false,
              },
          });
      }
      else if (swiper_count == 2) {
          mySwiperxyz = new Swiper(string_swiper_id, {
              slidesPerView: 1,
              spaceBetween: 30,
              centeredSlides: true,              
              autoplay: {
                  delay: 4000,
                  disableOnInteraction: false,
              },
          });
      }
     

      swiper_count += 1;


      if (document.getElementById(string_right_btn) != null) {
          document.getElementById(string_right_btn).addEventListener('click', function (event) {
              mySwiperxyz.slideNext();
          });
          document.getElementById(string_left_btn).addEventListener('click', function (event) {
              mySwiperxyz.slidePrev();
          });
      }

  }
})

var swiper = new Swiper(".mySwiper_mb", {
    slidesPerView: 1,
    spaceBetween: 30,
    autoplay: {
        delay: 4000,
        disableOnInteraction: false,
      },
    pagination: {
      el: ".swiper-pagination",
      clickable: true,
    },
    navigation: {
      nextEl: ".swiper-button-next",
      prevEl: ".swiper-button-prev",
    },
  });

  var swiper = new Swiper(".mySwiper_doitac", {
    slidesPerView: 5,
    spaceBetween: 30,
    autoplay: {
        delay: 4000,
        disableOnInteraction: false,
      },
    pagination: {
      el: ".swiper-pagination",
      clickable: true,
    },
    navigation: {
      nextEl: ".swiper-button-next",
      prevEl: ".swiper-button-prev",
    },
  });

  var swiper = new Swiper(".mySwiper_doitac2", {
    slidesPerView: 2.5,
    spaceBetween: 30,
    autoplay: {
        delay: 4000,
        disableOnInteraction: false,
      },
    pagination: {
      el: ".swiper-pagination",
      clickable: true,
    },
    navigation: {
      nextEl: ".swiper-button-next",
      prevEl: ".swiper-button-prev",
    },
  });

