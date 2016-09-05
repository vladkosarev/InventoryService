docker doesnt, from inside client, understand localhost or 127.0.0.1, it only understands 0.0.0.0 but it can accept localhost requesting from outside
akka receives 0.0.0.0 but drops it. it only processes localhost or 127.0.0.1
so we set host of actor system to 0.0.0.0 so docker is happy but we set the public host to 'localhost' so when client localhost comes from outside , docker can accept it and akka wont drop it, and external client is set to local host rather than 0.0.0.0 
but its almost imposible to have client from inside via docker linking 