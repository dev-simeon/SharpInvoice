using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System.Collections.Generic;

namespace SharpInvoice.API.Examples
{
    /// <summary>
    /// Provides example data for ProfileDto in Swagger documentation.
    /// </summary>
    public class ProfileDtoExample : IExamplesProvider<ProfileDto>
    {
        /// <summary>
        /// Gets example instance of ProfileDto.
        /// </summary>
        /// <returns>Sample ProfileDto data.</returns>
        public ProfileDto GetExamples()
        {
            return new ProfileDto(
                Id: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                FirstName: "John",
                LastName: "Doe",
                Email: "john.doe@example.com",
                AvatarUrl: "/avatars/john-doe.jpg",
                PhoneNumber: "+1 (555) 123-4567"
            );
        }
    }

    /// <summary>
    /// Provides example data for UpdateProfileDto in Swagger documentation.
    /// </summary>
    public class UpdateProfileDtoExample : IExamplesProvider<UpdateProfileDto>
    {
        /// <summary>
        /// Gets example instance of UpdateProfileDto.
        /// </summary>
        /// <returns>Sample UpdateProfileDto data.</returns>
        public UpdateProfileDto GetExamples()
        {
            return new UpdateProfileDto(
                FirstName: "John",
                LastName: "Doe",
                PhoneNumber: "+1 (555) 123-4567",
                AvatarUrl: "/avatars/john-doe.jpg"
            );
        }
    }

    /// <summary>
    /// Provides example data for TeamMemberDto in Swagger documentation.
    /// </summary>
    public class TeamMemberDtoExample : IExamplesProvider<TeamMemberDto>
    {
        /// <summary>
        /// Gets example instance of TeamMemberDto.
        /// </summary>
        /// <returns>Sample TeamMemberDto data.</returns>
        public TeamMemberDto GetExamples()
        {
            return new TeamMemberDto(
                Id: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                UserId: Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                UserName: "Jane Smith",
                Email: "jane.smith@example.com",
                RoleId: Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                RoleName: "Manager",
                JoinedAt: DateTime.UtcNow.AddDays(-30)
            );
        }
    }

    /// <summary>
    /// Provides example data for a list of TeamMemberDto in Swagger documentation.
    /// </summary>
    public class TeamMemberListExample : IExamplesProvider<List<TeamMemberDto>>
    {
        /// <summary>
        /// Gets example instance of a list of TeamMemberDto.
        /// </summary>
        /// <returns>Sample list of TeamMemberDto data.</returns>
        public List<TeamMemberDto> GetExamples()
        {
            return new List<TeamMemberDto>
            {
                new TeamMemberDto(
                    Id: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                    UserId: Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                    UserName: "Jane Smith",
                    Email: "jane.smith@example.com",
                    RoleId: Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                    RoleName: "Manager",
                    JoinedAt: DateTime.UtcNow.AddDays(-30)
                ),
                new TeamMemberDto(
                    Id: Guid.Parse("d4e5f6a7-b8c9-0123-def4-56789abcdef0"),
                    UserId: Guid.Parse("e5f6a7b8-c901-2345-6789-abcdef012345"),
                    UserName: "Bob Johnson",
                    Email: "bob.johnson@example.com",
                    RoleId: Guid.Parse("f6a7b8c9-0123-4567-89ab-cdef01234567"),
                    RoleName: "Viewer",
                    JoinedAt: DateTime.UtcNow.AddDays(-15)
                )
            };
        }
    }

    /// <summary>
    /// Provides example data for InviteTeamMemberRequest in Swagger documentation.
    /// </summary>
    public class InviteTeamMemberRequestExample : IExamplesProvider<InviteTeamMemberRequest>
    {
        /// <summary>
        /// Gets example instance of InviteTeamMemberRequest.
        /// </summary>
        /// <returns>Sample InviteTeamMemberRequest data.</returns>
        public InviteTeamMemberRequest GetExamples()
        {
            return new InviteTeamMemberRequest(
                Email: "new.member@example.com",
                RoleId: Guid.Parse("f6a7b8c9-0123-4567-89ab-cdef01234567")
            );
        }
    }

    /// <summary>
    /// Provides example data for UpdateTeamMemberRoleRequest in Swagger documentation.
    /// </summary>
    public class UpdateTeamMemberRoleRequestExample : IExamplesProvider<UpdateTeamMemberRoleRequest>
    {
        /// <summary>
        /// Gets example instance of UpdateTeamMemberRoleRequest.
        /// </summary>
        /// <returns>Sample UpdateTeamMemberRoleRequest data.</returns>
        public UpdateTeamMemberRoleRequest GetExamples()
        {
            return new UpdateTeamMemberRoleRequest(
                NewRoleId: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890")
            );
        }
    }
}