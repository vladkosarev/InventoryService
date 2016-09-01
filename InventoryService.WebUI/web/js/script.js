var app = angular.module("InventoryServiceApp", [
    "ngRoute", "jsonFormatter"
]);

app.config([
    "$routeProvider", function($routeProvider) {
        //$routeProvider
        //    // Home
        //    .when("/", {
        //        templateUrl: "partials/home.html" /*,controller: ""*/
        //    })
        //    // else 404
        //    .otherwise("/404", { templateUrl: "partials/404.html", controller: "PageCtrl" });
    }
]);
angular.module("InventoryServiceApp").factory("endpoints", function() {
    return {
        hub: "/signalr",
        webApi: "/api/values/ActorSystemStates"
    };
});
angular.module("InventoryServiceApp").service("service", function ($q, $http, $rootScope) {
    this.POST = function(url, item) {
        var deferred = $q.defer();
        var load = JSON.stringify(item);
        $http.post(url, load, {
                headers: {
                    'Content-Type': "application/json"
                }
            }).
            success(deferred.resolve).
            error(deferred.reject);
        $rootScope.allCurrentHttpPromises.push(deferred.promise);
        return deferred.promise;

    };
    this.GET = function(url) {

        var deferred = $q.defer();
        $http({
                method: "GET",
                url: url
            }).
            success(deferred.resolve).
            error(deferred.reject);
        $rootScope.allCurrentHttpPromises.push(deferred.promise);
        return deferred.promise;
    };
});
angular.module("InventoryServiceApp").factory("hub", function(endpoints, $timeout) {
    $.connection.hub.url = endpoints.hub;
    var chat = $.connection.InventoryServiceHub;
    return {
        ready: function(f) {
            $.connection.hub.start().done(function() {
                var arg = arguments;
                $timeout(function() {
                    f && f.apply(null, arg);
                });
            });
        },
        chat: chat,
        server: chat.server,
        client: function(name, f) {
            chat.client[name] = function(response) {
                var arg = arguments;
                $timeout(function() {
                    f && f.apply(null, arg);
                });
            };
        }
    };

});

angular.module("InventoryServiceApp").controller("ActorsCtrl", function($scope, $rootScope, $http, $q, $timeout, hub) {

   
    hub.client("inventoryData", function(response) {
  



        $("#jsGrid1")
            .jsGrid({
                width: "100%",
                height: "400px",

                inserting: false,
                editing: false,
                sorting: true,
                paging: false,

                data: response,

                fields: [
                    { name: "Name", type: "text", width: 150, validate: "required" },
                    { name: "Age", type: "number", width: 50 },
                    { name: "Address", type: "text", width: 200 },
                    // { name: "Country", type: "select", items: countries, valueField: "Id", textField: "Name" }
                ]
            });
    });

    hub.ready(function () {


    });
   
});