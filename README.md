# solis-scraper
This tool scrapes the status page of your Solis Wifi dongle and publishes the statistics from this page to MQTT for Home Assistant to consume.

The status page of your Solis Wifi dongle can be found at http://admin:admin@IP_OF_SOLIS_DEVICE/status.html

This tool is currently not configured to use TLS. If you need it, open an issue.

Usage
-----
```
docker run -d -e "Scraper__Host=http://<<IP_OF_SOLIS_DEVICE>>" -e "Mqtt__Host=<<MQTT_HOST_OR_IP>>" -e "Mqtt__Username=<<MQTT_USERNAME>>" -e "Mqtt__Password=<<MQTT_PASSWORD>>" ghcr.io/ikkentim/solis-scraper:main
```

Configuration
-------------
A couple of options are available:

- **`Scraper__Host`**: (required) URI of host to be scraped. e.g. http://SOME_IP_ADDRESS/
- **`Scraper__Username`**: Username for Solis device. Default: `admin`
- **`Scraper__Password`**: Password of Solis device. Default: `admin`
- **`Mqtt__Host`**: Host of the MQTT service.
- **`Mqtt__Username`**: Username of the MQTT service.
- **`Mqtt__Password`**: Password of the MQTT service.
- **`Mqtt__ClientId`**: Client ID used in connection with MQTT service. Default: `solis_scraper`
- **`Mqtt__DiscoveryPrefix`**: Prefix of used topics. Default: `homeassistant`
- **`Mqtt__NodeId`**: The node ID used in topics. Change this value if you're running multiple scrapers. Default: `solis`
- **`Mqtt__UniqueIdPrefix`**: The unique ID prepended to the sensor entities configured by this scraper.  Change this value if you're running multiple scrapers. Default: `solis_scraper`
