app.controller('AlertsController', function ($scope, $location, $http) {
    $scope.isActive = function (route) {
        return route === $location.path();
    }
    $scope.Logout = function () {
        document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
        $location.path('/');
    }
    $http.get('api/Account/Alerts?token=' + getAuthToken()).then(function (response) {
        if (response.data.length == 0) {
            $scope.NoResults = true;
            $scope.Results = [];
        }
        else {
            $scope.NoResults = false;
            $scope.Results = response.data;
            $http.post('api/Account/ViewAlerts?token=' + getAuthToken());
        }
    }, function (error) {
        if (error.status == 403)
            bootbox.alert(error.data.ExceptionMessage, function () {
                document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                $location.path('/');
            });
    });
    $scope.Alert = function (Result) {
        $http.post('api/Account/Alert?token=' + getAuthToken() + '&UserID=' + parseInt(Result.UserID)).then(function (response) {
            Result.Alerted = true;
        }, function (error) {
            if (error.status == 403)
                bootbox.alert(error.data.ExceptionMessage, function () {
                    document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                    $location.path('/');
                });
        });
    };
    $scope.Message = function (UserID) {
        var message = "";
        bootbox.prompt("New Message", function (result) {
            $(".input-block-level").changeElementType("input");
            $(".input-block-level").css("height", "30px");
            $(".input-block-level").bind("propertychange keyup input paste", function (e) { });
            $('.modal-footer .btn-primary').html('Ok');
            if (message != "") {
                $http.post('api/Account/SendMessage?token=' + getAuthToken(), { UserID: UserID, Message: message }).then(function (response) {
                    bootbox.alert('Your message has been sent!');
                }, function (error) {
                    if (error.status == 403)
                        bootbox.alert(error.data.ExceptionMessage, function () {
                            document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                            $location.path('/');
                        });
                });
            }
        });
        $(".input-block-level").changeElementType("textarea");
        $(".input-block-level").css("height", "300px");
        $(".input-block-level").bind("propertychange keyup input paste", function (e) {
            message = $(".input-block-level").val();
        });
        $('.modal-footer .btn-primary').html('Send');
    }
});

app.controller('NewAlertsController', function ($scope, $rootScope, $http) {
    $http.get('api/Account/NewAlerts?token=' + getAuthToken()).then(function (response) {
        $scope.NewAlerts = response.data;
    }, function (error) {
        $scope.NewAlerts = [];
        if (error.status == 403)
            bootbox.alert(error.data.ExceptionMessage, function () {
                document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                $location.path('/');
            });
    });
});