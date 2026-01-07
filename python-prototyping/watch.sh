while true; do
    inotifywait -e close_write ./python-prototyping/$1
    python3 ./python-prototyping/$1
done
