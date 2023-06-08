using Data.Model;
using Data.Response;

namespace Data.Mapper;

public static class CourseToCourseResponse
{
    public static CourseResponse ToCourseResponse(this Course course) =>
        new CourseResponse()
        {
            Id = course.Id,
            Lesson = course.Lesson.Title
        };

}