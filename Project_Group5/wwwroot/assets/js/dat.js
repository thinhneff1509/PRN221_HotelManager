const fakeInputFile = document.getElementById("fake-input");
const realInputFile = document.getElementById("fileCV");

fakeInputFile.addEventListener("click", (e) => {
  realInputFile.click();
});

realInputFile.addEventListener("change", (e) => {
  const file = e.target.files[0];
  if (file) document.getElementById("file-name").textContent = file.name;
});
