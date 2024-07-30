/**
 *
 */
Ext.define('MMIS.Utility', {
    singleton: true,

    /**
     * 顯示訊息於訊息欄
     * @param {String} msg 欲顯示訊息         
     */
    msglabel: function (msg) {
        var jMsgLabel = $('#msglabel-labelEl', top.document)[0];
        if (jMsgLabel) {
            jMsgLabel.innerHTML = msg
        }
    },

    /**
     * 取得URL參數，為key、value對應
     * @return {Object}     
     */
    getUrlParams: function (url) {
        var queryString;
        url = url || document.URL;
        queryString = url.split("?")[1] || '';

        return Ext.Object.fromQueryString(queryString);
    },

    /**
     * 開啟一個內嵌頁框的視窗
     * @param {String} title 視窗標題
     * @param {String} url 頁框URL
     */
    openIFrameWindow: function (title, url, config) {
        var me = this,
            config = config || {},
            cfgWidth = config.width,
            cfgHeight = config.height,
            win;
        
        delete config.width;
        delete config.height;

        config = Ext.apply({
            title: title,
            layout: 'fit',
            width: me.calcuateWidth(cfgWidth),
            height: me.calcuateHeight(cfgHeight),
            constrain: true,
            html: "<iframe src='" + url + "' height='100%' width='100%' frameborder='0'  style='background-color:#FFFFFF'></iframe>",
            buttons: [
                {
                    text: '關閉',
                    handler: function () {
                        this.up("window").close();
                    }
                }
            ]
        }, config);

        win = Ext.widget('window', config).show();

        Ext.EventManager.onWindowResize(function () {
            win.setWidth(me.calcuateWidth(cfgWidth));
            win.setHeight(me.calcuateHeight(cfgHeight));
            win.center();
        });

    },

    calculatePercentage: function (number, perscentage) {
        if (Ext.isString(perscentage) && perscentage.indexOf('%') !== -1) {
            return number * (+perscentage.replace('%', '') / 100);
        }
        return number * (+perscentage / 100);
    },

    calcuateWidth: function (w) {
        var bodySize = Ext.getBody().getViewSize(),
            isWidthPersentage = Ext.isString(w) && w.indexOf('%') !== -1;
        
        if (isWidthPersentage) {
            return this.calculatePercentage(bodySize.width, w)
        }
        return w || bodySize.width - 10;
    },

    calcuateHeight: function (h) {
        var bodySize = Ext.getBody().getViewSize(),
            isHeightPersentage = Ext.isString(h) && h.indexOf('%') !== -1;

        if (isHeightPersentage) {
            return this.calculatePercentage(bodySize.height, h)
        }
        return h || bodySize.height - 10;
    },

    /**
     * 下載檔案
     * @param {String} fg 上傳檔案編碼
     */
    downLoadFileByFG: function (fg) {
        window.open('/Download/Index/' + fg);
    },

    loadCss: function (src) {
        var css = document.createElement('link');
        css.setAttribute('type', 'text/css');
        css.setAttribute('rel', 'stylesheet');
        css.setAttribute('href', src);
        css.setAttribute('media', 'screen');

        document.getElementsByTagName('head')[0].appendChild(css);
    },

    openSubWindow: function (title, viewName, cfg, windowCfg) {
        var me = this,
            windowCfg = windowCfg || {},
            cfgWidth =  windowCfg.width || '80%',
            cfgHeight = windowCfg.height || '80%',
            win;

        delete windowCfg.width;
        delete windowCfg.height;

        windowCfg = Ext.apply({
            title: title,
            width: me.calcuateWidth(cfgWidth),
            height: me.calcuateHeight(cfgHeight),
            layout: 'fit',
            maximizable: true,
            constrain: true,
            items: Ext.create(viewName, cfg)
        }, windowCfg);

        win = Ext.widget("window", windowCfg).show();

        Ext.EventManager.onWindowResize(function () {
            var bodySize = Ext.getBody().getViewSize();
            win.setWidth(me.calcuateWidth(cfgWidth));
            win.setHeight(me.calcuateHeight(cfgHeight));
            win.center();
        });

        return win;
    },

    numberRenderer: Ext.util.Format.numberRenderer('0,0.##')
});