cd "$(dirname "$0")"

hexo clean
hexo g

# 将 public 里面的静态站点数据全部 copy 到外层,GitHub 上面会展示出来的
cp -rvf public/* ../

sleep 30s