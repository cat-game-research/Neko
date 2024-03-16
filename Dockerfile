# Use an official base image with Python and Miniconda
FROM continuumio/miniconda3

# Set the working directory to the root of your project
WORKDIR /Neko

# Copy the current directory contents into the container
COPY . /Neko

# Install any necessary dependencies
RUN conda env create -f environment.yml

# Make port 80 available to the world outside this container
EXPOSE 80

# Define the command to run your application
CMD ["./run.bat"]
