using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using System.ComponentModel;

namespace NestedCommands;

// A top-level command that lists all the values in the database. Since it inherits from
// BaseCommand, it has a Path argument even though no arguments are defined here
[Command("list")]
[Description("Lists all students and courses.")]
internal class ListCommand : BaseCommand
{
    protected override Task<int> RunAsync(Database db)
    {
        using var writer = LineWrappingTextWriter.ForConsoleOut();
        writer.WriteLine("Students:");
        foreach (var (id, student) in db.Students)
        {
            writer.WriteLine($"{id}: {student.LastName}, {student.FirstName}; major: {student.Major}");
            if (student.Courses.Count > 0)
            {
                writer.Indent = 4;
                writer.WriteLine("  Courses:");
                foreach (var course in student.Courses)
                {
                    string name = db.Courses.TryGetValue(course.CourseId, out var realCourse)
                        ? realCourse.Name
                        : $"Unknown ID {course.CourseId}";

                    writer.WriteLine($"{name}: grade {course.Grade}");
                }

                writer.Indent = 0;
                writer.ResetIndent();
            }
        }

        writer.WriteLine();
        writer.WriteLine("Courses:");
        foreach (var (id, course) in db.Courses)
        {
            writer.WriteLine($"{id}: {course.Name}; teacher: {course.Teacher}");
        }

        return Task.FromResult((int)ExitCode.Success);
    }
}
