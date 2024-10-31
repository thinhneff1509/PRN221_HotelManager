
// // setTimeout(function () {
//   document.getElementById("popup-overlay").style.display = "flex";
// }, 10000);

function openModal() {
  document.getElementById(
    "modalContainer"
  ).style.display = "flex";
  document.getElementById(
      "open_menu"
    ).style.display = "none";
    document.getElementById(
      "close_menu"
    ).style.display = "flex";
} 

function closeModalMobile() {
  // Ẩn popup
  document.getElementById(
    "modalContainer"
  ).style.display = "none";
  document.getElementById(
      "close_menu"
    ).style.display = "none";
    document.getElementById(
      "open_menu"
    ).style.display = "flex";
}


// document.getElementById("close-button")
// .addEventListener("click", function () {
//   // Ẩn popup
//   document.getElementById(
//     "popup-overlay"
//   ).style.display = "none";

// });


function scrollToTop() {
  const btn_up =
    document.getElementById("btn_up");
  window.scrollTo({
    top: 0,
    behavior: `smooth`,
  });
}
/*
function openTab(tabId) {
  // Ẩn tất cả nội dung tab
  const tabContents = document.querySelectorAll('.tab-content');
  tabContents.forEach(tabContent => {
    tabContent.style.display = 'none';
  });

  // Hiển thị tab được chọn
  const selectedTab = document.getElementById(tabId);
  if (selectedTab) {
    selectedTab.style.display = 'block';
  }
}
*/
function printClickedElement(event) {
    var clickedElement = event.target;
    var isAlreadyActive =
        clickedElement.classList.contains("active");

    if (!isAlreadyActive) {
        // Nếu phần tử chưa có lớp 'active', thì thêm lớp
        clickedElement.classList.add("active");

        // Xóa lớp 'active' từ tất cả các phần tử khác
        var allLinks =
            document.querySelectorAll("div a");
            console.log(allLinks)
        allLinks.forEach(function (link) {
            if (link !== clickedElement) {
                link.classList.remove("active");
            }
        });
    }
}





