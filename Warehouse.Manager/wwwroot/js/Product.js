const imgDiv = document.getElementById("img-preview-div");

$(document).ready(function () {
  var img = $('#set-b64').val();
  if (img != '') {
    imgDiv.style.display = "block";
  }
});

const convertBase64 = (file) => {
  return new Promise((resolve, reject) => {
    const fileReader = new FileReader();
    fileReader.readAsDataURL(file);
    fileReader.onload = () => {
      resolve(fileReader.result);
    };
    fileReader.onerror = (error) => {
      reject(error);
    };
  });
};
const convertByteArray = (file) => {
  return new Promise((resolve, reject) => {
    const fileReader = new FileReader();
    fileReader.readAsArrayBuffer(file);
    fileReader.onload = () => {
      resolve(fileReader.result);
    };
    fileReader.onerror = (error) => {
      reject(error);
    };
  });
};
const uploadImage = async (event) => {
  const file = event.target.files[0];
  const base64 = await convertBase64(file);
  //const byteArray = await convertByteArray(file);
  const resized = await reduce_image_file_size(base64);
  $('#set-b64').val(resized.substring(resized.indexOf(',')+1));
  document.getElementById("img-preview").setAttribute('src',resized);
};
async function base64ToArrayBuffer(base64) {
  var binary_string = window.atob(base64);
  var len = binary_string.length;
  var bytes = new Uint8Array(len);
  for (var i = 0; i < len; i++) {
    bytes[i] = binary_string.charCodeAt(i);
  }
  return bytes.buffer;
}
async function reduce_image_file_size(base64Str, MAX_WIDTH = 128, MAX_HEIGHT = 128) {
  let resized_base64 = await new Promise((resolve) => {
    let img = new Image();
    img.src = base64Str;
    img.onload = () => {
      let canvas = document.createElement('canvas');
      let width = img.width;
      let height = img.height;

      if (width > height) {
        if (width > MAX_WIDTH) {
          height *= MAX_WIDTH / width;
          width = MAX_WIDTH;
        }
      } else {
        if (height > MAX_HEIGHT) {
          width *= MAX_HEIGHT / height;
          height = MAX_HEIGHT;
        }
      }
      canvas.width = width;
      canvas.height = height;
      let ctx = canvas.getContext('2d');
      ctx.drawImage(img, 0, 0, width, height);
      resolve(canvas.toDataURL()); // this will return base64 image results after resize
    }
  });
  return resized_base64;
}

document.getElementById("get-file").addEventListener("change", (e) => {
  uploadImage(e);
  imgDiv.style.display = "block";
});