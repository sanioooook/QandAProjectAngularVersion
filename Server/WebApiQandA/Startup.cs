using AutoMapper;
using Entities.Interfaces;
using Entities.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApiQandA.Interfaces;
using WebApiQandA.Services;

namespace WebApiQandA
{
    public class Startup
	{
        private const string AngularCorsPolicy = "_angularCorsPolicy";

        public void ConfigureServices(IServiceCollection services)
		{
			const string connectionString = "Data Source=localhost;Initial Catalog=QandA;Integrated Security=True;";
			services.AddTransient<IUserRepository, UserRepository>(provider => new UserRepository(connectionString));
			services.AddTransient<ISurveyRepository, SurveyRepository>(provider => new SurveyRepository(connectionString));
			services.AddTransient<IAnswerRepository, AnswerRepository>(provider => new AnswerRepository(connectionString));
			services.AddTransient<IVoteRepository, VoteRepository>(provider => new VoteRepository(connectionString));
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ISurveyService, SurveyService>();
            services.AddTransient<IAnswerService, AnswerService>();
            services.AddTransient<IVoteService, VoteService>();
            services.AddAutoMapper(typeof(Startup).Assembly);

			services.AddCors(options =>
			{
				options.AddPolicy(name: AngularCorsPolicy, builder =>
				{ builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin(); });
			});

			services.AddControllersWithViews();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

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