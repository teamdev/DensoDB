function ShowAlert(message) {
  $(".alert > strong").html(message);
  $(".alert").attr("class","alert alert-success").css("display", "block");
}

function ShowWarning(message) {
  $(".alert > strong").html(message);
  $(".alert").attr("class", "alert alert-block").css("display", "block");
}

function ShowError(message) {
  $(".alert > strong").html(message);
  $(".alert").attr("class", "alert alert-error").css("display", "block");
}