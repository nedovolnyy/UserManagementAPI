namespace UserManagement.IntegrationTests.WebAPI.Controllers;

public class UserControllerTests
{
    [Test]
    public async Task GetUsers_WhenGetUsersThenResponseShouldBeExpectedUsers()
    {
        // Arrange
        var data = ReadFromFile("response_getUsers.json");
        var expected = JsonConvert.DeserializeObject<PagedList<User>>(data);

        // Act
        var response = await TestWebFixture.Client.GetAsync("/api/User/users");
        var actual = JsonConvert.DeserializeObject<PagedList<User>>(await response.Content.ReadAsStringAsync());

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task GetUsers_WhenGetUsersWithParamsThenResponseShouldBeExpectedUsers()
    {
        // Arrange
        var data = ReadFromFile("response_getUsersWithParams.json");
        var expected = JsonConvert.DeserializeObject<PagedList<User>>(data);

        // Act
        var response = await TestWebFixture.Client
            .GetAsync("/api/User/users?PageNumber=1&PageSize=8&FilterExpression=age%3E21&OrderBy=age&DisplayedFields=id%2Cname%2Cage%2Cemail%2Cpasswordhash%2Croles");
        var actual = JsonConvert.DeserializeObject<PagedList<User>>(await response.Content.ReadAsStringAsync());

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task GetUserById_WhenGetUserByIdThenResponseShouldBeExpectedUser()
    {
        // Arrange
        var data = ReadFromFile("response_getUserById.json");
        var expected = JsonConvert.DeserializeObject<User>(data);

        // Act
        var response = await TestWebFixture.Client.GetAsync("/api/User/user/1509fd70-c234-46fc-bdad-1d4ed54e4cb4");
        var actual = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task GetRoleById_WhenGetRoleByIdThenResponseShouldBeExpectedUser()
    {
        // Arrange
        var data = ReadFromFile("response_getRoleById.json");
        var expected = JsonConvert.DeserializeObject<string>(data);

        // Act
        var response = await TestWebFixture.Client.GetAsync("/api/User/role/1509fd70-c234-46fc-bdad-1d4ed54e4cb4");
        var actual = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    public string ReadFromFile(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"UserManagement.IntegrationTests.WebAPI.expectedJSONs.{fileName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
