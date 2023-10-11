namespace UserManagement.IntegrationTests.Database;

public class UserRepositoryTests
{
    private static readonly IUserRepository _userRepository = TestDatabaseFixture.ServiceProvider.GetRequiredService<IUserRepository>();
    private readonly List<User> _expectedUsers = new ()
    {
        new User
        {
            Id = "1509fd70-c234-46fc-bdad-1d4ed54e4cb4",
            Name = "Krot Artsiom",
            Age = 31,
            Email = "ADMIN@ADMIN.COM",
            PasswordHash = "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918",
            Roles = "Admin, SuperAdmin",
        },
        new User
        {
            Id = "b5a67ca7-cd15-4cad-ad23-002864c62a2b",
            Name = "Admin Examplovich",
            Age = 18,
            Email = "USER@EXAMPLE.COM",
            PasswordHash = "473287F8298DBA7163A897908958F7C0EAE733E25D2E027992EA2EDC9BED2FA8",
            Roles = "Admin, SuperAdmin",
        },
        new User
        {
            Id = "4ae0d943-f4e6-4c58-a6f6-002864c62a2b",
            Name = "Karpov Dima",
            Age = 48,
            Email = "newemail@TEST.COM",
            PasswordHash = "9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08",
            Roles = "Support",
        },
    };

    [Test]
    public async Task Insert_WhenInsertUser_ShouldBeEqualSameUser()
    {
        // act
        await _userRepository.InsertUserAsync(_expectedUsers[2]);
        var actualDbSet = TestDatabaseFixture.DatabaseContext.Users;

        // assert
        actualDbSet.Should().ContainEquivalentOf(_expectedUsers[0], op => op.ExcludingMissingMembers());
    }

    [Test]
    public async Task Update_WhenUpdateUser_ShouldBeEqualSameUser()
    {
        // arrange
        var expectedUser = _expectedUsers[2];

        // act
        await _userRepository.UpdateUserAsync(expectedUser);
        var actualUser = await _userRepository.GetUserByIdAsync(expectedUser.Id);

        // assert
        actualUser.Should().BeEquivalentTo(expectedUser);
    }

    [Test]
    public async Task Delete_WhenDeleteUser_ShouldStateDeleted()
    {
        // arrange
        var expectedCount = TestDatabaseFixture.DatabaseContext.Users.Count() - 1;

        // act
        await _userRepository.DeleteUserAsync(_expectedUsers[1].Id);
        var actualCount = TestDatabaseFixture.DatabaseContext.Users.Count();

        // assert
        actualCount.Should().Be(expectedCount);
    }

    [Test]
    public async Task GetById_WhenHaveIdEntry_ShouldEntryWithThisId()
    {
        // arrange
        var actualUserDbSet = TestDatabaseFixture.DatabaseContext.Users;

        // act
        var expectedUser = await _userRepository.GetUserByIdAsync(_expectedUsers[0].Id);

        // assert
        actualUserDbSet.Should().ContainEquivalentOf(expectedUser);
    }

    [Test]
    public void GetAllByLayoutId_WhenHaveEntry_ShouldContainThisUsers()
    {
        // arrange
        var actualUsers = TestDatabaseFixture.DatabaseContext.Users.ToList();

        // act
        var expectedUsers = _userRepository.GetAllUsersByRoles(_expectedUsers[2].Roles).ToList();

        // assert
        foreach (var user in expectedUsers)
        {
            actualUsers.Should().ContainEquivalentOf(user);
        }
    }
}
