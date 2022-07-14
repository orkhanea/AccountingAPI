using Accounting.Model;
using Accounting.Model.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Accounting.Helper
{
    public class HelperClass
    {
        public object GetUser(List<Tax> taxes = null, User user=null, List<User> users=null )
        {
            if (user!=null)
            {
                double persentage = 0;
                double salary = user.Position.Salary;
                double totalTaxes = 0;

                foreach (var tax in taxes.Where(t=>t.Type==user.Position.Company.CompanyType))
                {
                    var percent1 = tax.Persentage.ToString();
                    var percent2 = tax.Persentage2.ToString();
                    var minimum = tax.Minimum.ToString();
                    string name1 = tax.Name;


                    if (tax.Minimum != null)
                    {

                        if (salary > tax.Minimum)
                        {
                            var expr = tax.Formula2.Replace("salary", salary.ToString()).Replace("percent_1", percent1).Replace("percent_2", percent2).Replace("minimum", minimum);

                            var res = CSharpScript.EvaluateAsync(expr).Result.ToString();
                            totalTaxes += double.Parse(res);

                        }
                        else if (salary <= tax.Minimum && tax.Formula != null)
                        {
                            var expr = tax.Formula.Replace("salary", salary.ToString()).Replace("percent_1", percent1).Replace("percent_2", percent2).Replace("minimum", minimum);

                            var res = CSharpScript.EvaluateAsync(expr).Result.ToString();
                            totalTaxes += double.Parse(res);
                        }

                    }
                    else
                    {
                        if (tax.Formula != null && tax.Persentage != null)
                        {
                            var expr = tax.Formula.Replace("salary", salary.ToString()).Replace("percent_1", percent1).Replace("percent_2", percent2).Replace("minimum", minimum);
                            var res = CSharpScript.EvaluateAsync(expr).Result.ToString();
                            totalTaxes += double.Parse(res);
                        }


                    }

                }

                double totalSalary = salary - totalTaxes;

                var returnUser = new { Name = user.Name, Surname = user.Surname, CompanyName = user.Position.Company.CompanyName, Position = user.Position.PositionName, GrossSalary = user.Position.Salary, NetSalary = totalSalary };

                return returnUser;

            }

            if (users!=null)
            {
                List<AllUsersDetailsDTO> allUsersDetailsDTOs = new();

                foreach (var user1 in users)
                {
                    double persentage = 0;
                    double salary = user1.Position.Salary;
                    double totalTaxes = 0;

                    foreach (var tax in taxes.Where(t => t.Type == user1.Position.Company.CompanyType))
                    {
                        var percent1 = tax.Persentage.ToString();
                        var percent2 = tax.Persentage2.ToString();
                        var minimum = tax.Minimum.ToString();
                        string name1 = tax.Name;

                        if (tax.Minimum != null)
                        {

                            if (salary > tax.Minimum)
                            {
                                var expr = tax.Formula2.Replace("salary", salary.ToString()).Replace("percent_1", percent1).Replace("percent_2", percent2).Replace("minimum", minimum);

                                var res = CSharpScript.EvaluateAsync(expr).Result.ToString();
                                totalTaxes += double.Parse(res);

                            }
                            else if (salary <= tax.Minimum && tax.Formula != null)
                            {
                                var expr = tax.Formula.Replace("salary", salary.ToString()).Replace("percent_1", percent1).Replace("percent_2", percent2).Replace("minimum", minimum);

                                var res = CSharpScript.EvaluateAsync(expr).Result.ToString();
                                totalTaxes += double.Parse(res);
                            }

                        }
                        else
                        {
                            if (tax.Formula != null && tax.Persentage != null)
                            {
                                var expr = tax.Formula.Replace("salary", salary.ToString()).Replace("percent_1", percent1).Replace("percent_2", percent2).Replace("minimum", minimum);
                                var res = CSharpScript.EvaluateAsync(expr).Result.ToString();
                                totalTaxes += double.Parse(res);
                            }

                        }

                    }

                    double totalSalary = salary - totalTaxes;
                    AllUsersDetailsDTO allUsersDetailsDTO = new()
                    {
                        Name = user1.Name,
                        Surname = user1.Surname,
                        Bdate = user1.BDate,
                        CreatedDAte = user1.CreatedAt,
                        Email = user1.Email,
                        NettSalary = user1.Position.Salary,
                        GrossSalary = totalSalary,
                        Position = user1.Position.PositionName,
                        Company = user1.Position.Company.CompanyName
                    };

                    allUsersDetailsDTOs.Add(allUsersDetailsDTO);

                }

                return allUsersDetailsDTOs;
            }
            return null;
        }
    }
}
