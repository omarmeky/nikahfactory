var $root = $('html, body');

app.controller('LandingController', function ($scope, $anchorScroll, $routeParams, $http, $timeout) {
    var token = $routeParams.token; 
    if (token != null) {
        $http.post('api/Account/VerifyGuardianEmail?token=' + token).then(function (response) {
            $timeout(function(){
                bootbox.alert("Thank you for verifiying your email!", function () {
                    document.location.href = "/";
                });
            }, 500);
        }, function (error) {
            $timeout(function () {
                bootbox.alert("Oops, looks like an error occured. Please try again.", function () {
                    document.location.href = "/";
                });
            }, 500);
        });
    }
    $scope.scrollTo = function (id) {
        $root.animate({
            scrollTop: $(id).offset().top
        }, 900);
    }
});

app.controller('RegisterController', function ($scope, $http, $location) {
    $scope.processForm = function () {
        var birthday = new Date($('#birthdayRegistration').val());
        birthday = birthday.getTime();
        birthday = epochToTicks(birthday);
        $http.post('api/Account/Register', { FirstName: $scope.FirstName, LastName: $scope.LastName, Email: $scope.Email, Password: $scope.Password, Gender: $scope.Gender, GuardianEmail: $scope.GuardianEmail, Country: $scope.Country, State: $scope.State, Birthday: birthday, CreditCard: $scope.CreditCard, ExpirationMonth: $scope.ExpirationMonth, ExpirationYear: $scope.ExpirationYear, SecurityCode: $scope.SecurityCode}).then(function (response) {
            saveAuthToken(response.data.token);
            $location.path("/settings");
        },
        function (error) {
            bootbox.alert(error.data.ExceptionMessage);
        });
    };
});

app.controller('LoginController', function ($scope, $http, $location) {
    $scope.resetPassword = function ($scope) {
        bootbox.prompt("What is your email?", function (result) {
            if (result != null) {
                $http.post('api/Account/ResetPassword?email=' + result).then(function (response) {
                    bootbox.alert("We've emailed you a link with a temporary password.");
                },
                function (error) {
                    bootbox.alert(error.data.ExceptionMessage);
                });
            }
        });
    };
    $scope.processForm = function () {
        $http.post('api/Account/Login', { Email: $scope.Email, Password: $scope.Password }).then(function (response) {
            saveAuthToken(response.data.token);
            $location.path("/home");
        },
        function (error) {
            bootbox.alert(error.data.ExceptionMessage);
        });
    };
});

app.controller('ContactController', function ($scope, $http) {
    $scope.processForm = function () {
        $http.post('/api/Account/Contact', { Name: $scope.Name, Email: $scope.Email, Message: $scope.Message }).then(function (response) {
            $scope.Name = "";
            $scope.Email = "";
            $scope.Message = "";
            bootbox.alert("Your message has been received!");
        });
    };
});

function epochToTicks(e) {
    return (e * 10000) + 621355968000000000;
}
function ticksToEpoch(t) {
    return (t - 621355968000000000) / 10000;
}