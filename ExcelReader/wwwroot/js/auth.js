import { AUTH_LOGIN_URL, AUTH_REGISTER_ADMIN_URL, AUTH_REGISTER_USER_URL } from "./Config";
$(document).ready(() => {
    $("#sign-in-form").submit((e) => {
        e.preventDefault();


        let username = $("#inputEmail").val();
        let password = $("#inputPassword").val();


        let userData = {
            Username: username,
            Password: password
        };


        $.ajax({
            url: AUTH_LOGIN_URL,
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(userData),
            success: (data) => {
                localStorage.setItem('jwtToken', data.token);
                console.log("Login successful!");
                console.log(data);
                alert("Logged in Successfully")


            },
            error: (err) => {

                console.error("Login failed!");
                console.error(err.responseText);
                alert("Login failed. Please check your credentials.");
            }
        });
    });


    $("#sign-up-form").submit((e) => {
        e.preventDefault();

        let username = $("#inputEmail").val();
        let password = $("#inputPassword").val();
        let isAdmin = $("#isAdminCheckbox").is(":checked");

        let userData = {
            Username: username,
            Password: password,
            IsAdmin: isAdmin
        };

        let url = isAdmin ? AUTH_REGISTER_ADMIN_URL : AUTH_REGISTER_USER_URL;

        $.ajax({
            url: url,
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(userData),
            success: (data) => {
                console.log("Signup successful!");
                console.log(data);
                alert("Signup successful!" + `\n${data}`);
            },
            error: (err) => {
                console.error("Signup failed!");
                console.error(err.responseText);
                alert(`Signup failed: ${err.responseText}`);
            }
        });
    });
});