var app = angular.module("InventoryServiceApp", [
    "ngRoute", "jsonFormatter"
]);

app.config([
    "$routeProvider", function ($routeProvider) {
        //$routeProvider
        //    // Home
        //    .when("/", {
        //        templateUrl: "partials/home.html" /*,controller: ""*/
        //    })
        //    // else 404
        //    .otherwise("/404", { templateUrl: "partials/404.html", controller: "PageCtrl" });
    }
]);
angular.module("InventoryServiceApp").factory("endpoints", function () {
    return {
        hub: "/signalr",
        webApi: "/api/values/ActorSystemStates"
    };
});
angular.module("InventoryServiceApp").service("service", function ($q, $http, $rootScope) {
    this.POST = function (url, item) {
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
    this.GET = function (url) {
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
angular.module("InventoryServiceApp").factory("hub", function (endpoints, $timeout) {
    $.connection.hub.url = endpoints.hub;
    var inventoryServiceHub = $.connection.inventoryServiceHub;
    return {
        ready: function (f) {
            $.connection.hub.start().done(function () {
                var arg = arguments;
                $timeout(function () {
                    f && f.apply(null, arg);
                });
            });
        },
        inventoryServiceHub: inventoryServiceHub,
        server: inventoryServiceHub.server,
        client: function (name, f) {
            inventoryServiceHub.client[name] = function (response) {
                var arg = arguments;
                $timeout(function () {
                    f && f.apply(null, arg);
                });
            };
        }
    };
});

angular.module("InventoryServiceApp").controller("ActorsCtrl", function ($scope, $rootScope, $http, $q, $timeout, hub) {
    var messageCount = 0;
    $scope.model = {};
    $scope.model.logMessages = true;
    var storage = function(a,b) {
        if (typeof (Storage) !== "undefined") {
            if (b) {
                localStorage.setItem(a, b);
                return undefined;
            } else {
             return   localStorage.getItem(a);
            }
        } else {
            console.log("Sorry! No Web Storage support..");
            return undefined;
        }
    };
    var initStorage = function() {
        $scope.model.realtime=  storage("model.realtime")?true:false;
        $scope.model.logMessages = storage("model.logMessages") ? true : false;
    };
    $scope.updateStorage=function () {
        storage("model.realtime", $scope.model.realtime);
        storage("model.logMessages", $scope.model.logMessages);
    }
    initStorage();
    var hasLoaded = false;
    var lastResponse = {};
    var lastResponseDict = {};
    $scope.realTimeInventories = [];
    $scope. updateGrid = function () {
        $scope.newUpdateAvailable = 0;

        $scope.realTimeInventories = lastResponse.RealTimeInventories;
        //$("#jsGrid1")
        //    .jsGrid({
        //        width: "100%",
        //        height: "500px",
        //        inserting: false,
        //        editing: false,
        //        sorting: true,
        //        paging: false,

        //        data: lastResponse.RealTimeInventories,

        //        fields: [
        //            { name: "ProductId", type: "text", width: 200 },
        //            { name: "Quantity", type: "text", width: 200 },
        //            { name: "Reserved", type: "text", width: 200 },
        //            { name: "Holds", type: "text", width: 200 }
        //        ]
        //    });
    }
    $scope.newUpdateAvailable = 0;
    hub.client("inventoryData", function (response) {
        for (var i = 0; i < response.RealTimeInventories.length; i++) {
            var newInventory = response.RealTimeInventories[i];
            var productId = newInventory.ProductId;
            var cachedInv = lastResponseDict[productId];
            if (cachedInv) {
                if ((cachedInv.Quantity !== newInventory.Quantity) || (cachedInv.Reserved !== newInventory.Reserved) || (cachedInv.Holds !== newInventory.Holds)) {
                    $scope.newUpdateAvailable++;
                }
            } else {
                $scope.newUpdateAvailable++;
            }
            lastResponseDict[productId] = newInventory;
        }
        lastResponse = response;
        if ($scope.model.realtime || !hasLoaded) {
             $timeout(function() {
                 $scope.updateGrid();
                 hasLoaded = true;
             });
        }
    });

    $scope.serverNotificationMessages = "";
    $scope.messageSpeed = "";
    $scope.model.incomingMessages = [];

    hub.client("serverNotificationMessages",
        function(response) {
            $scope.serverNotificationMessages = response;
        });
    $scope.peekMessageCount = 0;
    hub.client("messageSpeed",
        function(response) {
            var speed = parseInt(response, 10);
            $scope.messageSpeed = speed;
            if ($scope.peekMessageCount < speed) {
                $scope.peekMessageCount = speed;
            }
        });
    $scope.maxMessageCount = 15;
    hub.client("incomingMessage",
        function (response) {
            messageCount++;
            $timeout(function() {
                $scope.model.incomingMessagesFirst =messageCount+" : "+ response;
                    if ($scope.model.logMessages) {
                    } else {
                        $scope.model.incomingMessages = [];
                    }
                    $timeout((function (incomingMessagesFirst) {
                    return function () {
                     incomingMessagesFirst && $scope.model.incomingMessages.unshift(incomingMessagesFirst);
                        $scope.model.incomingMessagesFirst = false;
                        if ($scope.model.incomingMessages.length > $scope.maxMessageCount) {
                            while ($scope.model.incomingMessages.length > $scope.maxMessageCount) {
                                $scope.model.incomingMessages.pop();
                            }
                        }
                    }
                })($scope.model.incomingMessagesFirst), 500);
            });
        });

    hub.ready(function () {
        hub.server.getInventoryList();
    });
});