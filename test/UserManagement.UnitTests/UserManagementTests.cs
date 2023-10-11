using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using UserManagement.Common.DI;
using UserManagement.Common.Entities;
using UserManagement.Common.Validation;
using UserManagement.UserManagementAPI.Controllers;
using UserManagement.UserManagementAPI.Helpers;

namespace UserManagement.BusinessLogic.UnitTests;

public class UserManagementTests
{
    private static readonly Mock<IUserRepository> _userRepository = new () { CallBase = true };
    private static readonly Mock<ILogger<UserController>> _logger = new ();
    private readonly UserController _userController = new (_userRepository.Object, _logger.Object);
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
            Id = "4ae0d943-f4e6-4c58-a6f6-50b300b8aab6",
            Name = "Karpov Valeriy",
            Age = 48,
            Email = "KARPOVVALERIY@TEST.COM",
            PasswordHash = "9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08",
            Roles = "Support",
        },
    };
    private int _timesApplyRuleCalled;

    [SetUp]
    protected void SetUp()
    {
        _userRepository.Setup(x => x.InsertUserAsync(It.IsAny<User>())).Callback(() => _timesApplyRuleCalled++);
        _userRepository.Setup(x => x.UpdateUserAsync(It.IsAny<User>())).Callback(() => _timesApplyRuleCalled++);
        _userRepository.Setup(x => x.DeleteUserAsync(It.IsAny<string>())).Callback(() => _timesApplyRuleCalled++);
        _userRepository.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(_expectedUsers[0]);
        _userRepository.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<User>());
        foreach (var user in _expectedUsers)
        {
            _userRepository.Setup(x => x.GetAllUsersByRoles(It.IsAny<string>())).Returns(_expectedUsers.Where(x => x.Roles == user.Roles).AsQueryable());
        }
    }

    [Test]
    public void Validate_WhenUserFieldAgeIsNegativeNumber_ShouldThrow()
    {
        // arrange
        var userExpected = new User
        {
            Id = _expectedUsers[0].Id,
            Name = _expectedUsers[0].Name,
            Age = -2,
            Email = _expectedUsers[0].Email,
            PasswordHash = _expectedUsers[0].PasswordHash,
            Roles = _expectedUsers[0].Roles,
        };
        var strException =
            "The field 'Age' of User is not correct!";

        // act
        var actualException = Assert.ThrowsAsync<ValidationException>(
                        async () => await ValidationHelper.ValidateAsync(userExpected, _userRepository.Object));

        // assert
        Assert.That(actualException.Message, Is.EqualTo(strException));
    }

    [Test]
    public void Validate_WhenUserFieldNameEmpty_ShouldThrow()
    {
        // arrange
        var userExpected = new User
        {
            Id = _expectedUsers[0].Id,
            Name = string.Empty,
            Age = -2,
            Email = _expectedUsers[0].Email,
            PasswordHash = _expectedUsers[0].PasswordHash,
            Roles = _expectedUsers[0].Roles,
        };
        var strException =
            "The field 'Name' of User is not allowed to be empty!";

        // act
        var actualException = Assert.ThrowsAsync<ValidationException>(
                        async () => await ValidationHelper.ValidateAsync(userExpected, _userRepository.Object));

        // assert
        Assert.That(actualException.Message, Is.EqualTo(strException));
    }

    [Test]
    public void Validate_WhenUserFieldAgeZero_ShouldThrow()
    {
        // arrange
        var userExpected = new User
        {
            Id = _expectedUsers[0].Id,
            Name = _expectedUsers[0].Name,
            Age = 0,
            Email = _expectedUsers[0].Email,
            PasswordHash = _expectedUsers[0].PasswordHash,
            Roles = _expectedUsers[0].Roles,
        };
        var strException =
            "The field 'Age' of User is not allowed to be null!";

        // act
        var actualException = Assert.ThrowsAsync<ValidationException>(
                        async () => await ValidationHelper.ValidateAsync(userExpected, _userRepository.Object));

        // assert
        Assert.That(actualException.Message, Is.EqualTo(strException));
    }

    [Test]
    public void Validate_WhenUserFieldEmailEmpty_ShouldThrow()
    {
        // arrange
        var userExpected = new User
        {
            Id = _expectedUsers[0].Id,
            Name = _expectedUsers[0].Name,
            Age = _expectedUsers[0].Age,
            Email = string.Empty,
            PasswordHash = _expectedUsers[0].PasswordHash,
            Roles = _expectedUsers[0].Roles,
        };
        var strException =
            "The field 'Email' of User is not allowed to be empty!";

        // act
        var actualException = Assert.ThrowsAsync<ValidationException>(
                        async () => await ValidationHelper.ValidateAsync(userExpected, _userRepository.Object));

        // assert
        Assert.That(actualException.Message, Is.EqualTo(strException));
    }

    [Test]
    public void Validate_WhenEmailNonUnique_ShouldThrow()
    {
        // arrange
        _userRepository.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(_expectedUsers[0]);

        var strException =
            "An user exists with this email!";

        // act
        var actualException = Assert.ThrowsAsync<ValidationException>(
                        async () => await ValidationHelper.ValidateAsync(_expectedUsers[0], _userRepository.Object));

        // assert
        Assert.That(actualException.Message, Is.EqualTo(strException));
    }

    [Test]
    public async Task Insert_WhenCallInsertUser_ShouldNotZeroCallback()
    {
        // arrange
        var userExpected = new User
        {
            Id = _expectedUsers[0].Id,
            Name = _expectedUsers[0].Name,
            Age = _expectedUsers[0].Age,
            Email = "unique@email.com",
            PasswordHash = _expectedUsers[0].PasswordHash,
            Roles = _expectedUsers[0].Roles,
        };

        // act
        await _userController.CreateUserAsync(userExpected, "password*****");

        // assert
        Assert.NotZero(_timesApplyRuleCalled);
        _timesApplyRuleCalled = default;
    }

    [Test]
    public async Task Update_WhenCallUpdateUser_ShouldNotZeroCallback()
    {
        // arrange
        var userExpected = new User
        {
            Id = _expectedUsers[0].Id,
            Name = _expectedUsers[0].Name,
            Age = _expectedUsers[0].Age,
            Email = "8iuiub9889unique@email.com",
            PasswordHash = _expectedUsers[0].PasswordHash,
            Roles = _expectedUsers[0].Roles,
        };

        // act
        await _userController.UpdateUserAsync(userExpected, "newpassword*****");

        // assert
        Assert.NotZero(_timesApplyRuleCalled);
        _timesApplyRuleCalled = default;
    }

    [Test]
    public async Task Delete_WhenCallDeleteUser_ShouldNotZeroCallback()
    {
        // act
        await _userController.DeleteUserAsync(_expectedUsers[2].Id);

        // assert
        Assert.NotZero(_timesApplyRuleCalled);
        _timesApplyRuleCalled = default;
    }

    [Test]
    public async Task GetById_WhenReturnUserById_ShouldNotNull()
    {
        // act
        var actual = await _userController.GetByIdUserAsync(_expectedUsers[2].Id);

        // assert
        Assert.NotNull(actual);
    }
}
