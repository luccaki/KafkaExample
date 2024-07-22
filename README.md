# KafkaExample

Application to test Kafka functionalities

# Producer

Web API recieves HTTP request with message and send it to Kafka, must keep HTTP connection open and wait for a response from the consumer

# Consumer

Console app, consumes message from kafka and send a new message with the processed response

# Solution

Created a Correlation Id (GUID) that tracks response, we send the message with an Id, and waits for a response with the same Id

# How to run

Just need to run ```docker-compose up```

OpenAPI will be avialable in http://localhost:1357/swagger/index.html
