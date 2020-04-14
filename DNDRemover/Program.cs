using System;
using System.Collections;
using System.Text;
using GameMaker.GM8Project;

namespace DNDRemover
{
    class Program
    {
        static void Main(string[] args)
        {
            var filename = "k3.gmk";
            var reader = new ProjectReader(filename);

            Console.WriteLine("Reading project...");
            var project = reader.ReadProject();

            Console.WriteLine("Removing DND...");
            foreach (var obj in project.Objects)
            {
                foreach (var ev in obj.Events)
                {
                    foreach (var ev2 in ev)
                    {
                        DNDRemover.RemoveDNDForEvent(project, ev2);
                        //foreach (var action in ev2.Actions)
                        //{
                        //    //foreach (var a in action.Arguments)
                        //    //{
                        //    //    Console.WriteLine(a.Value);
                        //    //}
                        //    //Console.WriteLine(action.LibraryId);
                        //}
                    }
                }
            }

            Console.WriteLine("Writing project...");
            var writer = new ProjectWriter("RemovedDND.gmk", project);
            writer.WriteProject();
        }

    }

    class DNDRemover
    {
        public static void RemoveDNDForEvent(GameMaker.ProjectCommon.Project project, GameMaker.ProjectCommon.Event ev)
        {
            var remover = new ActionRemover(project);

            foreach (var action in ev.Actions)
            {
                remover.Begin(action);

                switch ((ActionID)action.ActionId)
                {
                    #region Move
                    case ActionID.StartMoving:
                        remover.AddLine($"direction = {action.Arguments[0].Value};");
                        remover.AddLine($"speed {remover.RelativeSign} {action.Arguments[1].Value};");
                        break;
                    case ActionID.SetDirection:
                        remover.AddLine($"direction {remover.RelativeSign} {action.Arguments[0].Value};");
                        remover.AddLine($"speed {remover.RelativeSign} {action.Arguments[1].Value};");
                        break;
                    case ActionID.MoveTowardsPoint:
                        remover.AddLine($"move_towards_point({remover.RelativeOnly("x + ")}{action.Arguments[0].Value}, {remover.RelativeOnly("y + ")}{action.Arguments[1].Value}, {action.Arguments[2].Value});");
                        break;
                    case ActionID.SetHorizontalSpeed:
                        remover.AddLine($"hspeed {remover.RelativeSign} {action.Arguments[0].Value};");
                        break;
                    case ActionID.SetVerticalSpeed:
                        remover.AddLine($"vspeed {remover.RelativeSign} {action.Arguments[0].Value};");
                        break;
                    case ActionID.SetGravity:
                        remover.AddLine($"gravity_direction {remover.RelativeSign} {action.Arguments[0].Value};");
                        remover.AddLine($"gravity {remover.RelativeSign} {action.Arguments[1].Value};");
                        break;
                    case ActionID.ReverseHorizontalDirection:
                        remover.AddLine($"hspeed *= -1;");
                        break;
                    case ActionID.ReverseVerticalDirection:
                        remover.AddLine($"vspeed *= -1;");
                        break;
                    case ActionID.SetFriction:
                        remover.AddLine($"friction {remover.RelativeSign} {action.Arguments[0].Value};");
                        break;
                    case ActionID.JumpToPosition:
                        remover.AddLine($"x {remover.RelativeSign} {action.Arguments[0].Value};");
                        remover.AddLine($"y {remover.RelativeSign} {action.Arguments[1].Value};");
                        break;
                    case ActionID.JumpToStartPosition:
                        remover.AddLine($"x = xstart;");
                        remover.AddLine($"y = ystart;");
                        break;
                    case ActionID.JumpToRandomPosition:
                        remover.AddLine($"x = random(room_width);");
                        remover.AddLine($"y = random(room_height);");
                        remover.AddLine($"move_snap({action.Arguments[0].Value}, {action.Arguments[1].Value});");
                        break;
                    case ActionID.AlignToGrid:
                        remover.AddLine($"move_snap({action.Arguments[0].Value}, {action.Arguments[1].Value});");
                        break;
                    case ActionID.WarpWhenOutside:
                        var warpX = action.Arguments[0].Value == "0" || action.Arguments[0].Value == "2";
                        var warpY = action.Arguments[0].Value == "1" || action.Arguments[0].Value == "2";
                        remover.AddLine($"move_warp({(warpX ? "true" : "false")}, {(warpY ? "true" : "false")}, 0);");
                        break;
                    case ActionID.MoveToContact:
                        var funcName = "move_contact_solid";
                        if (action.Arguments[2].Value == "1")
                            funcName = "move_contact_all";
                        remover.AddLine($"{funcName}({action.Arguments[0].Value}, {action.Arguments[1].Value});");
                        break;
                    case ActionID.BounceAgainst:
                        funcName = "move_bounce_solid";
                        if (action.Arguments[1].Value == "1")
                            funcName = "move_bounce_all";
                        remover.AddLine($"{funcName}({action.Arguments[0].Value});");
                        break;
                    case ActionID.SetPath:
                        remover.AddLine($"path_start({action.Arguments[0].Value}, {action.Arguments[1].Value}, {action.Arguments[2].Value}, {action.Arguments[3].Value});");
                        break;
                    case ActionID.EndPath:
                        remover.AddLine($"path_end();");
                        break;
                    case ActionID.SetPathPosition:
                        remover.AddLine($"path_position {remover.RelativeSign} {action.Arguments[0].Value};");
                        break;
                    case ActionID.SetPathSpeed:
                        remover.AddLine($"path_speed {remover.RelativeSign} {action.Arguments[0].Value};");
                        break;
                    case ActionID.StepTowardPoint:
                        remover.AddLine($"mp_linear_step({action.Arguments[0].Value}, {action.Arguments[1].Value}, {action.Arguments[2].Value}, {action.Arguments[3].Value});");
                        break;
                    case ActionID.StepAvoiding:
                        remover.AddLine($"mp_potential_step({action.Arguments[0].Value}, {action.Arguments[1].Value}, {action.Arguments[2].Value}, {action.Arguments[3].Value});");
                        break;
                    #endregion
                    #region Main 1
                    case ActionID.CreateInstance:
                        var objName = action.Arguments[0].Value;
                        remover.AddLine($"instance_create({remover.RelativeOnly("x + ")}{action.Arguments[1].Value}, {remover.RelativeOnly("y + ")}{action.Arguments[2].Value}, {objName});");
                        break;
                    case ActionID.CreateMovingInstance:
                        objName = action.Arguments[0].Value;
                        remover.AddLine($"with (instance_create({remover.RelativeOnly("x + ")}{action.Arguments[1].Value}, {remover.RelativeOnly("y + ")}{action.Arguments[2].Value}, {objName}))");
                        remover.AddLine("{");
                        remover.AddLine($"    speed {remover.RelativeSign} {action.Arguments[3].Value};");
                        remover.AddLine($"    direction {remover.RelativeSign} {action.Arguments[4].Value};");
                        remover.AddLine("}");
                        break;
                    case ActionID.CreateRandomInstance:
                        objName = $"{project.Objects[int.Parse(action.Arguments[0].Value)].Name} || {project.Objects[int.Parse(action.Arguments[1].Value)].Name} || {project.Objects[int.Parse(action.Arguments[2].Value)].Name} || {project.Objects[int.Parse(action.Arguments[3].Value)].Name}";
                        remover.AddLine($"instance_create({remover.RelativeOnly("x + ")}{action.Arguments[4].Value}, {remover.RelativeOnly("y + ")}{action.Arguments[5].Value}, {objName});");
                        break;
                    case ActionID.ChangeInstanceInto:
                        objName = action.Arguments[0].Value;
                        remover.AddLine($"instance_change({objName}, {action.Arguments[0].Value});");
                        break;
                    case ActionID.DestroyInstance:
                        remover.AddLine($"instance_destroy();");
                        break;
                    case ActionID.DestroyInstanceAtPosition:
                        var x = action.Arguments[0].Value;
                        var y = action.Arguments[1].Value;
                        remover.AddLine($"while (instance_position({remover.RelativeOnly("x + ")}{x}, {remover.RelativeOnly("y + ")}{y}, all) != noone)");
                        remover.AddLine("{");
                        remover.AddLine($"    with (instance_position({remover.RelativeOnly("x + ")}{x}, {remover.RelativeOnly("y + ")}{y}, all))");
                        remover.AddLine($"        instance_destroy();");
                        remover.AddLine("}");
                        break;
                    case ActionID.ChangeSpriteInto:
                        var sprName = action.Arguments[0].Value;
                        remover.AddLine($"sprite_index = {sprName};");
                        remover.AddLine($"image_index = {action.Arguments[1].Value};");
                        remover.AddLine($"image_speed = {action.Arguments[2].Value};");
                        break;
                    case ActionID.TransformSprite:
                        var xscale = int.Parse(action.Arguments[0].Value) * (action.Arguments[3].Value == "1" || action.Arguments[3].Value == "3" ? -1 : 1);
                        var yscale = int.Parse(action.Arguments[1].Value) * (action.Arguments[3].Value == "2" || action.Arguments[3].Value == "3" ? -1 : 1);

                        remover.AddLine($"image_xscale = {xscale};");
                        remover.AddLine($"image_yscale = {yscale};");
                        remover.AddLine($"image_angle = {action.Arguments[2].Value};");
                        break;
                    case ActionID.SetSpriteColorBlending:
                        remover.AddLine($"image_blend = {action.Arguments[0].Value};");
                        remover.AddLine($"image_alpha = {action.Arguments[1].Value};");
                        break;
                    case ActionID.PlaySound:
                        funcName = "sound_play";
                        if (action.Arguments[1].Value == "1")
                            funcName = "sound_loop";
                        remover.AddLine($"{funcName}({action.Arguments[0].Value});");
                        break;
                    case ActionID.StopSound:
                        remover.AddLine($"sound_stop({action.Arguments[0].Value});");
                        break;
                    case ActionID.IfSoundIsPlaying:
                        remover.AddCondition($"if (sound_is_playing({action.Arguments[0].Value})");
                        break;
                    case ActionID.GoToNextRoom:
                        remover.AddLine($"transition_kind = {action.Arguments[0].Value};");
                        remover.AddLine($"room_goto_previous();");
                        break;
                    case ActionID.RestartCurrentRoom:
                        remover.AddLine($"transition_kind = {action.Arguments[0].Value};");
                        remover.AddLine($"room_restart();");
                        break;
                    case ActionID.GoToRoom:
                        remover.AddLine($"transition_kind = {action.Arguments[0].Value};");
                        remover.AddLine($"room_goto({action.Arguments[1].Value});");
                        break;
                    case ActionID.IfPreviousRoomExists:
                        remover.AddCondition($"if (room_previous(room) != -1)");
                        break;
                    case ActionID.IfNextRoomExists:
                        remover.AddCondition($"if (room_next(room) != -1)");
                        break;

                    #endregion
                    #region Main 2
                    case ActionID.SetAlarm:
                        remover.AddLine($"alarm[{action.Arguments[1].Value}] {remover.RelativeSign} {action.Arguments[0].Value};");
                        break;
                    case ActionID.Sleep:
                        remover.AddLine($"sleep({action.Arguments[0].Value});");
                        if (action.Arguments[0].Value == "1")
                            remover.AddLine($"screen_redraw();");
                        break;
                    case ActionID.SetTimeLine:
                        remover.AddLine($"timeline_index = {action.Arguments[0].Value};");
                        remover.AddLine($"timeline_position = {action.Arguments[1].Value};");
                        break;
                    case ActionID.SetTimeLinePosition:
                        remover.AddLine($"timeline_position = {action.Arguments[0].Value};");
                        break;
                    case ActionID.DisplayMessage:
                        remover.AddLine($"show_message({action.Arguments[0].Value});");
                        break;
                    case ActionID.ShowGameInfo:
                        remover.AddLine($"show_info({action.Arguments[0].Value});");
                        break;
                    case ActionID.ShowVideo:
                        remover.AddLine($"splash_set_fullscreen({action.Arguments[1].Value});");
                        remover.AddLine($"splash_show_video({action.Arguments[0].Value}, {action.Arguments[2].Value});");
                        break;
                    case ActionID.RestartGame:
                        remover.AddLine($"game_restart();");
                        break;
                    case ActionID.EndGame:
                        remover.AddLine($"game_end();");
                        break;
                    case ActionID.SaveGame:
                        remover.AddLine($"game_save({action.Arguments[0].Value});");
                        break;
                    case ActionID.LoadGame:
                        remover.AddLine($"game_load({action.Arguments[0].Value});");
                        break;
                    case ActionID.ReplaceFromSprite:
                        remover.AddLine($"sprite_replace({action.Arguments[0].Value}, {action.Arguments[1].Value}, {action.Arguments[2].Value}, 0, 0, sprite_get_xoffset({action.Arguments[0].Value}), sprite_get_yoffset({action.Arguments[0].Value}));");
                        break;
                    case ActionID.ReplaceFromSound:
                        remover.AddLine($"sound_replace({action.Arguments[0].Value}, {action.Arguments[1].Value}, 0, 0);");
                        break;
                    case ActionID.ReplaceFromBackground:
                        remover.AddLine($"background_replace_background({action.Arguments[0].Value}, {action.Arguments[1].Value});");
                        break;
                    #endregion
                    #region Control
                    case ActionID.IfPositionCollisionFree:
                        funcName = "place_free";
                        if (action.Arguments[2].Value == "1")
                            funcName = "place_empty";
                        remover.AddCondition($"if ({remover.NotSign}{funcName}({remover.RelativeOnly("x + ")}{action.Arguments[0].Value}, {remover.RelativeOnly("y + ")}{action.Arguments[1].Value}))");
                        break;
                    case ActionID.IfCollisionAtPosition:
                        funcName = "place_free";
                        if (action.Arguments[2].Value == "1")
                            funcName = "place_empty";
                        remover.AddCondition($"if ({(remover.NotSign == "!" ? "" : "!")}{funcName}({remover.RelativeOnly("x + ")}{action.Arguments[0].Value}, {remover.RelativeOnly("y + ")}{action.Arguments[1].Value}))");
                        break;
                    case ActionID.IfObjectAtPosition:
                        remover.AddCondition($"if ({remover.NotSign}position_meeting({remover.RelativeOnly("x + ")}{action.Arguments[1].Value}, {remover.RelativeOnly("y + ")}{action.Arguments[2].Value}, {action.Arguments[0].Value}))");
                        break;
                    case ActionID.IfNumberInstancesIsValue:
                        var op = action.Arguments[2].Value switch
                        {
                            "0" => "==",
                            "1" => "<",
                            "2" => ">",
                            _ => throw new Exception("Unknown operation"),
                        };
                        remover.AddCondition($"if ({remover.NotSign}instance_number({action.Arguments[0].Value}) {op} {action.Arguments[1].Value})");
                        break;
                    case ActionID.IfUserAnswerYes:
                        remover.AddCondition($"if ({remover.NotSign}show_question({action.Arguments[0].Value}))");
                        break;
                    case ActionID.IfExpressionIsTrue:
                        remover.AddCondition($"if ({remover.NotSign}({action.Arguments[0].Value}))");
                        break;
                    case ActionID.IfMouseButtonIsPressed:
                        var button = action.Arguments[0].Value switch
                        {
                            "0" => "noone",
                            "1" => "mb_left",
                            "2" => "mb_right",
                            "3" => "mb_middle",
                            _ => throw new Exception("Unknown mouse button"),
                        };
                        remover.AddCondition($"if ({remover.NotSign}mouse_check_button({button}))");
                        break;
                    case ActionID.IfInstanceIsAlignedWithGrid:
                        remover.AddCondition($"if ({remover.NotSign}place_snapped({action.Arguments[0].Value}, {action.Arguments[1].Value}))");
                        break;
                    case ActionID.StartBlock:
                        remover.AddLine("{");
                        break;
                    case ActionID.Else:
                        remover.AddCondition($"else");
                        break;
                    case ActionID.Exit:
                        remover.AddLine($"exit;");
                        break;
                    case ActionID.EndBlock:
                        remover.AddLine("}");
                        break;
                    case ActionID.Repeat:
                        remover.AddCondition($"repeat ({action.Arguments[0].Value})");
                        break;
                    case ActionID.CallInheritedEvent:
                        remover.AddLine($"event_inherited();");
                        break;
                    case ActionID.ExecuteCode:
                        foreach (var ln in action.Arguments[0].Value.Split('\n'))
                        {
                            remover.AddLine(ln);
                        }
                        break;
                    case ActionID.ExecuteScript:
                        remover.AddLine($"{action.Arguments[0].Value}({action.Arguments[1].Value}, {action.Arguments[2].Value}, {action.Arguments[3].Value}, {action.Arguments[4].Value}, {action.Arguments[5].Value});");
                        break;
                    case ActionID.Comment:
                        remover.AddLine($"// {action.Arguments[0].Value}");
                        break;
                    case ActionID.SetVariable:
                        remover.AddLine($"{action.Arguments[0].Value} {remover.RelativeSign} {action.Arguments[1].Value};");
                        break;
                    case ActionID.IfVariable:
                        op = action.Arguments[2].Value switch
                        {
                            "0" => "==",
                            "1" => "<",
                            "2" => ">",
                            _ => throw new Exception("Unknown operation"),
                        };
                        remover.AddCondition($"if ({remover.NotSign}{action.Arguments[0].Value} {op} {action.Arguments[1].Value})");
                        break;
                    case ActionID.DrawVariable:
                        remover.AddLine($"draw_text({remover.RelativeOnly("x + ")}{action.Arguments[1].Value}, {remover.RelativeOnly("y + ")}{action.Arguments[2].Value}, {action.Arguments[0].Value});");
                        break;
                        #endregion
                }

                remover.Finish();
            }

            var codeAction = remover.Apply();
            ev.Actions.Clear();
            ev.Actions.Add(codeAction);
        }

        class ActionRemover
        {
            StringBuilder code;
            int indentLevel = 0;
            GameMaker.ProjectCommon.Action action;
            GameMaker.ProjectCommon.Project project;

            Stack blockStack = new Stack();
            bool isCondition = false;

            public ActionRemover(GameMaker.ProjectCommon.Project project)
            {
                this.project = project;
                this.code = new StringBuilder();
            }

            public void Begin(GameMaker.ProjectCommon.Action action)
            {
                // Begin an action
                this.action = action;
                isCondition = false;

                // Applies to
                if (action.CanApplyTo)
                {
                    if (action.AppliesTo == -1)
                    {
                        // Self, do nothing
                    }
                    else if (action.AppliesTo == -2)
                    {
                        // Other
                        AddIndentCode($"with (other)");
                        AddIndentCode("{");
                        IncreaseIndent();
                    }
                    else
                    {
                        // Object
                        AddIndentCode($"with ({action.AppliesTo})");
                        AddIndentCode("{");
                        IncreaseIndent();
                    }
                }
            }

            public void AddLine(string command)
            {
                if (action.ActionKind == GameMaker.ActionKind.BeginGroup)
                {
                    blockStack.Push('{');
                    return;
                }
                if (action.ActionKind == GameMaker.ActionKind.EndGroup)
                {
                    return;
                }

                AddIndentCode(command);
            }

            public void AddCondition(string command)
            {
                isCondition = true;
                AddLine(command);
            }

            public void AddIndentCode(string command)
            {
                AddIndent();
                code.Append(command + "\n");
            }

            public void Finish()
            {
                // Finish an action
                if (isCondition)
                {
                    // Condition action
                    AddIndentCode("{");
                    IncreaseIndent();

                    blockStack.Push('}');
                }
                else if (action.ActionKind != GameMaker.ActionKind.BeginGroup && action.ActionKind != GameMaker.ActionKind.EndGroup)
                {
                    // Normal action
                    if (blockStack.Count != 0)
                    {
                        var top = (char)blockStack.Peek();
                        if (top == '}')
                        {
                            while (blockStack.Count != 0 && (char)blockStack.Peek() != '{')
                            {
                                blockStack.Pop();

                                DecreaseIndent();
                                AddIndentCode("}");
                            }
                        }
                    }
                }
                else if (action.ActionKind == GameMaker.ActionKind.EndGroup)
                {
                    // End group action
                    blockStack.Pop();
                    while (blockStack.Count != 0 && (char)blockStack.Peek() != '{')
                    {
                        blockStack.Pop();

                        DecreaseIndent();
                        AddIndentCode("}");
                    }
                }

                // Apply to
                if (action.CanApplyTo && action.AppliesTo != -1)
                {
                    DecreaseIndent();
                    AddIndentCode("}");
                }
            }

            public void AddIndent()
            {
                // Add indent
                for (var i = 0; i < indentLevel; i++)
                    code.Append("    ");
            }

            public void IncreaseIndent()
            {
                indentLevel++;
            }

            public void DecreaseIndent()
            {
                indentLevel--;
            }

            public string RelativeSign
            {
                get => $"{(action.Relative ? "+" : "")}=";
            }

            public string NotSign
            {
                get => $"{(action.Not ? "!" : "")}";
            }

            public string RelativeOnly(string content)
            {
                return action.Relative ? content : "";
            }

            public GameMaker.ProjectCommon.Action Apply()
            {
                return new GameMaker.ProjectCommon.Action()
                {
                    ActionId = (int)ActionID.ExecuteCode,
                    ActionKind = GameMaker.ActionKind.Code,
                    AllowRelative = false,
                    CanApplyTo = true,
                    ExecutionType = GameMaker.ExecutionType.Code,
                    LibraryId = 1,
                    AppliesTo = -1,
                    Not = false,
                    Relative = false,
                    Question = false,
                    ExecuteCode = "",
                    Arguments = new GameMaker.ProjectCommon.Argument[1]
                    {
                        new GameMaker.ProjectCommon.Argument()
                        {
                            Type = GameMaker.ArgumentType.String,
                            Value = code.ToString(),
                        }
                    }
                };
            }
        }

        enum ActionID
        {
            // Move
            StartMoving = 101,
            SetDirection = 102,
            MoveTowardsPoint = 105,
            SetHorizontalSpeed = 103,
            SetVerticalSpeed = 104,
            SetGravity = 107,
            ReverseHorizontalDirection = 113,
            ReverseVerticalDirection = 114,
            SetFriction = 108,
            JumpToPosition = 109,
            JumpToStartPosition = 110,
            JumpToRandomPosition = 111,
            AlignToGrid = 117,
            WarpWhenOutside = 112,
            MoveToContact = 116,
            BounceAgainst = 115,
            SetPath = 119,
            EndPath = 124,
            SetPathPosition = 122,
            SetPathSpeed = 123,
            StepTowardPoint = 120,
            StepAvoiding = 121,

            // Main 1
            CreateInstance = 201,
            CreateMovingInstance = 206,
            CreateRandomInstance = 207,
            ChangeInstanceInto = 202,
            DestroyInstance = 203,
            DestroyInstanceAtPosition = 204,
            ChangeSpriteInto = 541,
            TransformSprite = 542,
            SetSpriteColorBlending = 543,
            PlaySound = 211,
            StopSound = 212,
            IfSoundIsPlaying = 213,
            GoToPreviousRoom = 221,
            GoToNextRoom = 222,
            RestartCurrentRoom = 223,
            GoToRoom = 224,
            IfPreviousRoomExists = 225,
            IfNextRoomExists = 226,

            // Main 2
            SetAlarm = 301,
            Sleep = 302,
            SetTimeLine = 303,
            SetTimeLinePosition = 304,
            DisplayMessage = 321,
            ShowGameInfo = 322,
            ShowVideo = 323,
            RestartGame = 331,
            EndGame = 332,
            SaveGame = 333,
            LoadGame = 334,
            ReplaceFromSprite = 803,
            ReplaceFromSound = 804,
            ReplaceFromBackground = 805,

            // Control
            IfPositionCollisionFree = 401,
            IfCollisionAtPosition = 402,
            IfObjectAtPosition = 403,
            IfNumberInstancesIsValue = 404,
            WithChance1OutOf2 = 405,
            IfUserAnswerYes = 407,
            IfExpressionIsTrue = 408,
            IfMouseButtonIsPressed = 409,
            IfInstanceIsAlignedWithGrid = 410,
            StartBlock = 422,
            Else = 421,
            Exit = 425,
            EndBlock = 424,
            Repeat = 423,
            CallInheritedEvent = 604,
            ExecuteCode = 603,
            ExecuteScript = 601,
            Comment = 605,
            SetVariable = 611,
            IfVariable = 612,
            DrawVariable = 613,

            // Score
            SetScoreTo = 701,
            IfScoreIs = 702,
            DrawScore = 703,
            ShowHighscoreTable = 709,
            ClearHighscoreTable = 707,
            SetLivesTo = 711,
            IfLivesAreEqualTo = 712,
            DrawNumberOfLives = 713,
            DrawLivesAsImage = 714,
            SetHealth = 721,
            IfHealthIsEqualTo = 722,
            DrawHealthBar = 723,
            SetScoreCaptionInfo = 731,

            // Extra
            CreateParticleSystem = 820,
            DestroyParticleSystem = 821,
            ClearAllParticles = 822,
            CreateParticleType = 823,
            SetColorForType = 824,
            SetLifeForType = 826,
            SetMotionForType = 827,
            SetGravityForType = 828,
            SetSecondaryForType = 829,
            CreateEmitter = 831,
            DestroyEmitter = 832,
            BurstTypeFromEmitter = 833,
            StreamTypeFromEmitter = 834,
            PlayCDFromTrack = 808,
            StopCD = 809,
            PauseCD = 810,
            ResumeCD = 811,
            IfCDExists = 812,
            IfCDIsPlaying = 813,
            SetMouseTo = 801,
            OpenURL = 807,

            // Draw
            DrawSprite = 501,
            DrawBackground = 502,
            DrawText = 514,
            DrawTextTransformed = 519,
            DrawRectangle = 511,
            DrawHorGradient = 516,
            DrawVertGradient = 517,
            DrawEllipse = 512,
            DrawGradientEllipse = 518,
            DrawLine = 513,
            DrawArrow = 515,
            SetColor = 524,
            SetFont = 526,
            ChangeFullScreen = 531,
            TakeSnapshot = 802,
            CreateExplosionAt = 532,
        }
    }
}
