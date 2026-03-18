using System.Collections.Generic;

/// <summary>
/// 씬 간 컷씬 데이터 전달용 static 컨테이너.
/// 에디터 씬 → CutScene 씬으로 커맨드 리스트를 넘긴다.
/// 복귀 시 에디터 노드 상태를 복원하기 위한 데이터도 보관한다.
/// </summary>
public static class CutSceneData
{
    /// <summary>재생할 커맨드 목록</summary>
    public static List<TsvCommand> Commands;

    /// <summary>재생 후 돌아갈 씬 이름</summary>
    public static string ReturnSceneName;

    /// <summary>에디터 복귀 시 노드 그래프를 복원할 커맨드 목록</summary>
    public static List<TsvCommand> EditorCommands;

    /// <summary>에디터 복귀 시 StoryDatabase의 resourcesPath 복원용</summary>
    public static string EditorResourcesPath;
}
