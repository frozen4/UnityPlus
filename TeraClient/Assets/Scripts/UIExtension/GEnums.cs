public enum UVFillMethod
{
    SymmetryLR,     //Left-Right symmetry
    SymmetryUD,     //up-down
    SymmetryLRUD,    //2 dir
    Contain,   //Keep Aspect based on width
    None
}

public enum SortType
{
    Position,
    Priority
}
public enum Align
{
    Top = 1,
    Bottom,
    Center,
    Left,
    Right
}

public enum NewAlign
{
    Top = 1,
    Bottom,
    Center,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}

public enum PanelCloseType
{
    ClickAnyWhere,
    ClickEmpty,
    None,
    Tip,
}

public enum PanelSortingLayer
{
    GameWorld = 1,  //BottomMost
    RootPanel = 2,  //Bottom
    SubPanel = 3,   //Normal
    Dialog = 4,
    NormalTip = 5,  //Top
    Guide = 6,  //Guide
    ImportantTip = 7,   //Topmost
    Debug = 8   //Debug
}

public enum PanelOrderType
{
    Fixed,
    Floating
}