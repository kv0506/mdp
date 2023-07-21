# MDP
Movie Search Platform API

This is a ASP.NET Core based Web Api application.

To run this, you first need to do the following

1. Grab a API key from https://www.omdbapi.com/ and set it in the appsettings.json
2. Register a new project in the https://console.cloud.google.com/ and enable Youtube Data API v3. Then create an API key and set it in the appsettings.json
3. Now, build and run the application

There are two API endpoints

1. movie/search?query= -> This API provides the list of movies matching the given query
2. movie/{title} -> This API provides the complete details about the movie and related videos from Youtube
