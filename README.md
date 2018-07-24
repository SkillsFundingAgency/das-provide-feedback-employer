**TODO: Add build badge**

# Provide Feedback (Alpha)

This solution represents the Provider Feedback code base currently in alpha.

## Developer setup

### Requirements

* [.NET Core SDK >= 2.1.302](https://www.microsoft.com/net/download/)
* [Docker for X](https://docs.docker.com/install/#supported-platforms)

### Environment Setup

The default development environment uses docker containers to host it's dependencies.

* Redis
* Elasticsearch
* Logstash
* MongoDb

On first setup run the following command from _**/setup/containers/**_ to create the docker container images:

`docker-compose build`

To start the containers run:

`docker-compose up -d`

You can view the state of the running containers using:

`docker ps -a`

### Application logs
Application logs are logged to [Elasticsearch](https://www.elastic.co/products/elasticsearch) and can be viewed using [Kibana](https://www.elastic.co/products/kibana) at http://localhost:5601

## License

Licensed under the [MIT license](LICENSE)
