/**
 * 包含查詢功能與按鈕的表單
 */
Ext.define('MMIS.form.QueryForm', {
    extend: 'MMIS.form.Form',
    alias: 'widget.queryform',

    /**
     * @cfg {Ext.data.Store/Ext.data.Store[]} store (required)
     */

    /**
     * @cfg {String} defaultButtonsSite
     * 功能按鈕預設位置，可以使用的選項包含：
     * 
     * - **near** : 緊貼於欄位的右方靠下
     * - **bottom** : 置於表單右下方
     * - **dock** : 置於所有欄位的下方靠右
     */
    defaultButtonsSite: 'near',

    /**
     * @cfg {Boolean} showDefaultButtons
     * 是否顯示功能按鈕
     */
    showDefaultButtons: true,

    /**
     * @cfg {Boolean} allFieldRequire
     * 是否需輸入所有查詢條件
     */
    allFieldRequire: false,

    queryButton: null,

    resetButton: null,

    queryButtonHandler: function () { this.doQuery(); },

    resetButtonHandler: function () {
        var me = this;
        me.getForm().reset();
        me.fireEvent("clear", me);
    },

    hasEmptyValue: function () {
        var me = this, rtn = false;
        Ext.iterate(me.getValues(), function (key, value) {
            if (!value) {
                rtn = true;
                return false;
            }
        });
        return rtn;
    },

    doQuery: function () {
        var me = this,
            store = me.store,
            queryValues = me.getValues();

        if (me.allFieldRequire && me.hasEmptyValue()) {
            Ext.Msg.alert('提示', '請輸入所有查詢條件');
            return;
        }

        if (!me.isValid()) {
            Ext.Msg.alert('提示', '輸入格式錯誤');
            return;
        }

        if (me.fireEvent("beforequery", queryValues) === false) {
            return;
        }

        if (Ext.typeOf(store) !== 'array' && store) {
            store = [store];
        }

        Ext.iterate(queryValues, function (key, value) {
            if (Ext.typeOf(value) == 'array') {
                value = value.join(',');
            }
            Ext.each(store, function (s) {
                s.proxy.setExtraParam(key, value);
            });
        });

        Ext.each(store, function (s) {
            s.loadPage(1);
        });

        me.fireEvent("afterquery", queryValues);
    },


    getQueryButton: function () {
        return this.queryButton;
    },

    getResetButton: function () {
        return this.resetButton;
    },

    initComponent: function () {
        var me = this;

        me.addEvents({
            /**
             * @event beforequery
             * Fires before query
             * @param {Object} queryValues
             */
            beforequery: true,

            /**
             * @event afterquery
             * Fires after query
             * @param {Object} queryValues
             */
            afterquery: true,

            /**
             * @event clear
             * Fires after clear form
             * @param {MMIS.form.QueryForm} form
             */
            clear: true
        });

        if (Ext.typeOf(me.store) === 'string') {
            me.store = Ext.data.StoreManager.lookup(me.store || 'ext-empty-store');
        }

        if (Ext.typeOf(me.store) === 'array') {
            Ext.each(me.store, function (s, i) {
                if (Ext.typeOf(s) === 'string') {
                    me.store[i] = Ext.data.StoreManager.lookup(s || 'ext-empty-store');
                }
            });
        }

        me.callParent(arguments);
    },

    beforeInitComponent: function () {
        var me = this;

        me.callParent(arguments);

        if (me.showDefaultButtons) {
            me.prepareDefaultButtons();
        }
    },

    prepareDefaultButtons: function () {
        var me = this,
            buttonsSite = me.defaultButtonsSite;

        me.queryButton = me.generateQueryButton();
        me.resetButton = me.generateResetButton();

        if (Ext.typeOf(buttonsSite) == 'string') {
            buttonsSite = { site: buttonsSite };
            me.defaultButtonsSite = buttonsSite;
        }

        me.putDefaultButtons();
    },

    generateQueryButton: function () {
        var me = this;
        return Ext.widget("button", {
            text: '查詢',
            iconCls: 'TRASearch',
            scope: me,
            listeners: {
                click: me.queryButtonHandler,
                scope: me
            }
        });
    },

    generateResetButton: function () {
        var me = this;
        return Ext.widget("button", {
            text: '清除',
            iconCls: 'TRAClear',
            scope: me,
            handler: me.resetButtonHandler
        });
    },

    putDefaultButtons: function () {
        var me = this,
            buttonsSite = me.defaultButtonsSite;

        switch (buttonsSite.site) {
            case 'near':
                me.layout = 'hbox';
                me.items.push({
                    xtype: 'container',
                    padding: buttonsSite.padding,
                    height: '100%',
                    layout: {
                        type: 'vbox',
                        pack: 'end'
                    },
                    items: [{
                        xtype: 'container',
                        layout: {
                            type: 'hbox'
                        },
                        items: [me.queryButton, me.resetButton]
                    }]
                }

                    );
                break;
            case 'bottom':
                me.items.push({
                    xtype: 'container',
                    width: 750,
                    layout: {
                        type: 'hbox',
                        pack: 'end'
                    },
                    items: [me.queryButton, me.resetButton]
                });
                break;
            case 'dock':
                me.buttons = [me.queryButton, me.resetButton];
                break;
        }
    },

    addStore: function (store) {
        var me = this;

        store = Ext.data.StoreManager.lookup(store || 'ext-empty-store');

        if (!me.store) {
            me.store = [];
        }

        if (Ext.typeOf(me.store) !== 'array') {
            me.store = [me.store];
        }

        me.store.push(store);
    }
});
