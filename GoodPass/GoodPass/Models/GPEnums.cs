namespace GoodPass.Models;

/// <summary>
/// AddDataDialog的处理结果
/// </summary>
public enum AddDataResult
{
    Success,
    Failure_Duplicate,
    Failure,
    Undetermined
}
/// <summary>
/// AddDataSQL处理结果
/// </summary>
public enum SQLAddDataResult
{
    Success,
    Failure_Duplicate,
    Failure,
    Undetermined
}
/// <summary>
/// EditDataSQL处理结果
/// </summary>
public enum SQlEditDataResult
{
    Success,
    Failure_Duplicate,
    Failure,
    Undetermined
}

/// <summary>
/// EditDataDialog的处理结果
/// </summary>
public enum EditDataResult
{
    Success,
    Failure,
    UnknowError,
    Nochange
}

public enum OOBESituation
{
    EnableOOBE,
    DIsableOOBE
}

public enum AgreeStatus
{
    Agree,
    NotAgree
}

public enum PassportSignInResult
{
    Verified,
    Busy,
    Disabled,
    NotUseable,
    Failed,
    Cancel
}