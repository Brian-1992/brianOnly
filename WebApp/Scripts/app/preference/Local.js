/** 儲存個人偏好資訊 */
Ext.define('MMIS.preference.Local', {

    /** 
     * @cfg {String} id
     * 偏好ID
     */

    /** 
     * @cfg {String} userId
     * 
     */

    /** 
     * @cfg {Object} defaults
     * 預設偏好
     */

    /** 
     * @cfg {Boolean} clearExist
     * 使否於初始時清除現有的偏好設定
     */
    clearExist: false,

    constructor: function (config) {

        var me = this,
            getUserId = function () {
                return config.userId || top.userId || 'unknown';
            },
            storageKey = config.id + '-' + getUserId(),
            value = localStorage.getItem(storageKey),
            defaults = config.defaults || {};

        if (config.clearExist) {
            localStorage.removeItem(storageKey);
        }

        if (!localStorage.getItem(storageKey)) {
            localStorage.setItem(storageKey, JSON.stringify(defaults));
        }

        Ext.apply(me, config);
        me.storageKey = storageKey;

        me.callParent(arguments);
    },

    set: function (key, value) {
        var me = this,
            storageKey = me.storageKey,
            data = JSON.parse(localStorage.getItem(storageKey));

        data[key] = value;
        localStorage.setItem(storageKey, JSON.stringify(data));
    },

    get: function (key, defaultValue) {
        var data = this.getAll();

        return data && (data[key] || data[key] === "") ? data[key] : defaultValue;
    },

    getAll: function () {
        var data = localStorage.getItem(this.storageKey);
        return data ? JSON.parse(data) : data;
    },

    clear: function (torough) {
        if (torough) {
            localStorage.removeItem(this.storageKey);
            return;
        }
        localStorage.setItem(this.storageKey, JSON.stringify({}));
    },

    getStorageKey: function () {
        return this.storageKey;
    }
});