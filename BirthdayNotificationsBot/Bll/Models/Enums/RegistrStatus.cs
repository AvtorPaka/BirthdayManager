namespace BirthdayNotificationsBot.Bll.Models.Enums;
public enum RegistrStatus
{
    NewUser,
    NeedToFillWishes,
    EditDateOfBirth,
    EditUserWishes,
    FullyRegistrated,
    CreatingNewGroup,
    JoiningExistingGroup,
    ChoosingGroupToManage,
    ChoosingUserToGiveModeratorAccess,
    ChoosingUserToKickOutFromTheGroup,
    EditGroupName,
    EditGroupAdditionalInfo,
    ErrorState
}