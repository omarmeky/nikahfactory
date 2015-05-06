app.controller('HomeController', function ($scope, $location, $http, $timeout) {
    $scope.isActive = function (route) {
        return route === $location.path();
    }
    $scope.Logout = function () {
        document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
        $location.path('/');
    }
    $http.get('api/Account/MyProfile?token=' + getAuthToken()).then(function (response) {
        $scope.FirstName = response.data.FirstName;
        $scope.Age = response.data.Age;
        $scope.Heading = response.data.Heading;
        $scope.State = response.data.State;
        $scope.Country = response.data.Country;
        $scope.Bio = response.data.Bio;
        if (response.data.Gender == 'Brother')
            $scope.Image = 'images/male.png';
        else
            $scope.Image = 'images/female.png';
        $scope.EditingHeading = false;
        $scope.EditingBio = false;
    }, function (error) {
        if (error.status == 403)
            bootbox.alert(error.data.ExceptionMessage, function () {
                document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                $location.path('/');
            });
    });
    $scope.EditHeading = function () {
        $timeout(function () {
            $scope.EditingHeading = true;
        }, 1);
    };
    $scope.SaveHeading = function () {
        if ($scope.Heading != "") 
            $http.post('api/Account/SaveHeading?token=' + getAuthToken() + '&heading=' + $scope.Heading).then(function (response) {
                $scope.EditingHeading = false;
            }, function (error) {
                if (error.status == 403)
                    bootbox.alert(error.data.ExceptionMessage, function () {
                        document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                        $location.path('/');
                    });
            });
    };
    $scope.EditBio = function () {
        $timeout(function () {
            $scope.EditingBio = true;
        }, 1);
    };
    $scope.SaveBio = function () {
        if ($scope.Bio != "") 
            $http.post('api/Account/SaveBio?token=' + getAuthToken() + '&bio=' + $scope.Bio).then(function (response) {
                $scope.EditingBio = false;
            }, function (error) {
                if (error.status == 403)
                    bootbox.alert(error.data.ExceptionMessage, function () {
                        document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                        $location.path('/');
                    });
            });
    };
});