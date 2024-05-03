Graylog: http://localhost:9000/search
	system -> inputs -> GELF UDP -> Launch new input -> default

Jaeger UI: http://localhost:16686

Open Telemetry: http://localhost:5183/metrics

Prometheus: http://localhost:9090/graph
	http://host.docker.internal:5183/metrics	UP

	system_runtime_cpu_usage
	orders_service_grpc_response_time_sum
	orders_service_grpc_response_time_count
	orders_service_grpc_response_time_bucket

	orders_service_orders_cancelled_from_status
	orders_service_orders_cancellation_result

Graphana: http://localhost:3000
	http://localhost:3000/connections/datasources/new
	admin/admin
	http://prometheus:9090
