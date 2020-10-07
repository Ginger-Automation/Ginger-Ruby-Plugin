#
if ARGV.length != 2
  puts "We need exactly two arguments"
  exit
end

# input the numbers and converting them into integer
num1=ARGV[0].to_i
num2=ARGV[1].to_i

# finding sum
sum=num1+num2

# printing the result
puts "Result = #{sum}"