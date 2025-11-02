-- PostgreSQL initialization script for TechBirdsFly microservices
-- This script creates databases for all microservices

-- Create databases
CREATE DATABASE techbirdsfly_auth WITH ENCODING 'UTF8';
CREATE DATABASE techbirdsfly_eventbus WITH ENCODING 'UTF8';
CREATE DATABASE techbirdsfly_billing WITH ENCODING 'UTF8';
CREATE DATABASE techbirdsfly_generator WITH ENCODING 'UTF8';
CREATE DATABASE techbirdsfly_admin WITH ENCODING 'UTF8';
CREATE DATABASE techbirdsfly_user WITH ENCODING 'UTF8';
CREATE DATABASE techbirdsfly_image WITH ENCODING 'UTF8';
CREATE DATABASE techbirdsfly_gateway WITH ENCODING 'UTF8';

-- Grant all privileges
GRANT ALL PRIVILEGES ON DATABASE techbirdsfly_auth TO postgres;
GRANT ALL PRIVILEGES ON DATABASE techbirdsfly_eventbus TO postgres;
GRANT ALL PRIVILEGES ON DATABASE techbirdsfly_billing TO postgres;
GRANT ALL PRIVILEGES ON DATABASE techbirdsfly_generator TO postgres;
GRANT ALL PRIVILEGES ON DATABASE techbirdsfly_admin TO postgres;
GRANT ALL PRIVILEGES ON DATABASE techbirdsfly_user TO postgres;
GRANT ALL PRIVILEGES ON DATABASE techbirdsfly_image TO postgres;
GRANT ALL PRIVILEGES ON DATABASE techbirdsfly_gateway TO postgres;

-- Connection limit for production databases
ALTER DATABASE techbirdsfly_auth CONNECTION LIMIT 100;
ALTER DATABASE techbirdsfly_eventbus CONNECTION LIMIT 100;
ALTER DATABASE techbirdsfly_billing CONNECTION LIMIT 100;
ALTER DATABASE techbirdsfly_generator CONNECTION LIMIT 100;
ALTER DATABASE techbirdsfly_admin CONNECTION LIMIT 100;
ALTER DATABASE techbirdsfly_user CONNECTION LIMIT 100;
ALTER DATABASE techbirdsfly_image CONNECTION LIMIT 100;
ALTER DATABASE techbirdsfly_gateway CONNECTION LIMIT 100;
