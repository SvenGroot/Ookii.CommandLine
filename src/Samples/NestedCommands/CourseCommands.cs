﻿using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Validation;
using System.ComponentModel;

namespace NestedCommands;

// This is the top-level "course" command. It has no functionality since everything is done in the
// ParentCommand class.
[Command("course")]
[Description("Add or remove a course.")]
internal class CourseCommand : ParentCommand
{
}

// Command to add courses. Since it inherits from BaseCommand, it has a Path argument in addition
// to the arguments created here.
[GeneratedParser]
[Command("add")]
[ParentCommand(typeof(CourseCommand))]
[Description("Adds a course to the database.")]
internal partial class AddCourseCommand : BaseCommand
{
    [CommandLineArgument(IsPositional = true)]
    [Description("The name of the course.")]
    [ValidateNotWhiteSpace]
    public required string Name { get; set; }

    [CommandLineArgument(IsPositional = true)]
    [Description("The name of the teacher of the course.")]
    [ValidateNotWhiteSpace]
    public required string Teacher { get; set; }

    protected override async Task<int> RunAsync(Database db, CancellationToken cancellationToken)
    {
        int id = db.Courses.Any() ? db.Courses.Keys.Max() + 1 : 1;
        db.Courses.Add(id, new Course(Name, Teacher));
        await db.Save(Path, cancellationToken);
        Console.WriteLine($"Added a course with ID {id}.");
        return (int)ExitCode.Success;
    }
}

// Command to remove courses. Since it inherits from BaseCommand, it has a Path argument in addition
// to the arguments created here.
[GeneratedParser]
[Command("remove")]
[ParentCommand(typeof(CourseCommand))]
[Description("Removes a course from the database.")]
internal partial class RemoveCourseCommand : BaseCommand
{

    [CommandLineArgument(IsPositional = true)]
    [Description("The ID of the course to remove.")]
    public required int Id { get; set; }

    protected override async Task<int> RunAsync(Database db, CancellationToken cancellationToken)
    {
        if (db.Students.Any(s => s.Value.Courses.Any(c => c.CourseId == Id)))
        {
            Console.WriteLine("Can't remove a course referenced by a student.");
            return (int)ExitCode.IdError;
        }

        if (!db.Courses.Remove(Id))
        {
            Console.WriteLine("No such course");
            return (int)ExitCode.IdError;
        }

        await db.Save(Path, cancellationToken);
        Console.WriteLine("Course removed.");
        return (int)ExitCode.Success;
    }
}
