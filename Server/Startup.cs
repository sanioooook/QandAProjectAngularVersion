using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebApiQandA.Models.Interfaces;
using WebApiQandA.Models.Repositorys;

namespace Server2
{
	public class Startup
	{
		readonly string AngularCorsPolicy = "_angularCorsPolicy";
		public void ConfigureServices(IServiceCollection services)
		{
			string connectionString = "Data Source=localhost;Initial Catalog=QandA;Integrated Security=True;";
			services.AddTransient<IUserRepository, UserRepository>(provider => new UserRepository(connectionString));
			services.AddTransient<ISurveyRepository, SurveyRepository>(provider => new SurveyRepository(connectionString));
			services.AddTransient<IAnswerRepository, AnswerRepository>(provider => new AnswerRepository(connectionString));
			services.AddTransient<IVoteRepository, VoteRepository>(provider => new VoteRepository(connectionString));
			services.AddCors(options =>
			{
				options.AddPolicy(name: AngularCorsPolicy, builder =>
				{ builder.WithOrigins("http://localhost:4200").AllowAnyHeader(); });
			});

			services.AddControllersWithViews();
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseDeveloperExceptionPage();

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseCors(AngularCorsPolicy);

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}