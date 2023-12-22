using Microsoft.AspNetCore.Mvc.Testing;
using Server.Models;
using Newtonsoft.Json;
using System.Text;
using FluentAssertions;
using Server.Controllers;
using System.Net;


namespace WebTests;

public class BertControllerTests
{

    static public StringContent RequestToJson(Request input) 
    {
        string request_json = JsonConvert.SerializeObject(input);
        return new StringContent(request_json, Encoding.UTF8,  "application/json");
    }

    [Fact]
    public async Task POST_request()
    {
        await using var application = new WebApplicationFactory<Server.Startup>();
        using var client = application.CreateClient();

        string text = File.ReadAllText("..\\..\\..\\Test.txt");
        Request request = new(text);
        string question = "What is the story about?";
        request.Questions.Add(question);
        var request_full = RequestToJson(request);
        var response = await client.PostAsync("/Bert", request_full);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var response_body = JsonConvert.DeserializeObject<Response>(
          await response.Content.ReadAsStringAsync()
        );
        response_body.Should().NotBe(null);
        response_body!.Answers[0].Should().Be("how a baggins had an adventure");
    }
}