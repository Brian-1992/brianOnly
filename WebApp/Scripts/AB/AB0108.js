Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // 拆解網址參數
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var T1GetExcel = '/api/AB0108/Excel';
    var view_all = Ext.getUrlParam('view_all');

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var mLabelWidth = 80;
    var mWidth = 200;

    var whnoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function getWhnoCombo() {
        Ext.Ajax.request({
            url: '/api/AB0108/GetWhnoCombo',
            method: reqVal_p,
            params: { view_all: view_all },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var whnos = data.etts;
                    if (whnos.length > 0) {
                        whnoStore.add({
                            VALUE: '',
                            TEXT: '全部'
                        });
                        for (var i = 0; i < whnos.length; i++) {
                            whnoStore.add({
                                VALUE: whnos[i].VALUE,
                                TEXT: whnos[i].TEXT
                            });
                        }
                        T1Query.getForm().findField('P3').setValue('');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getWhnoCombo();

    function getStatusCombo() {
        Ext.Ajax.request({
            url: '/api/AB0108/GetStatusCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var status = data.etts;
                    if (status.length > 0) {
                        statusStore.add({
                            VALUE: '',
                            TEXT: '全部'
                        });
                        for (var i = 0; i < status.length; i++) {
                            statusStore.add({
                                VALUE: status[i].VALUE,
                                TEXT: status[i].TEXT
                            });
                        }
                        T1Query.getForm().findField('P2').setValue('');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getStatusCombo();


    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'DATA_DATE',
            'DATA_BTIME',
            'DATA_ETIME',
            'WH_NO',
            'MMCODE',
            'VISIT_KIND',
            'CONSUME_QTY',
            'STOCK_UNIT',
            'INSU_QTY',
            'HOSP_QTY',
            'PARENT_ORDERCODE',
            'PARENT_CONSUME_QTY',
            'CREATEDATETIME',
            'PROC_ID',
            'PROC_MSG',
            'PROC_TYPE'
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'CREATE_TIME', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            timeout: 180000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0108/GetData',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    view_all: view_all
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '刷條碼時間',
                    name: 'P0',
                    id: 'P0',
                    width: 160,
                    padding: '0 4 0 4',
                    //format: 'Xm',
                    fieldCls: 'required',
                    value: new Date((new Date()).getFullYear(), (new Date()).getMonth(), 1)
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P1',
                    id: 'P1',
                    // enforceMaxLength: true,
                    //maxLength: 5,
                    //minLength: 5,
                    //regexText: '請填入民國年月',
                    //regex: /\d{5,5}/,
                    fieldCls: 'required',
                    labelWidth: 8,
                    labelSeperator: '',
                    width: 88,
                    padding: '0 4 0 4',
                    //format: 'Xm',
                    value: new Date()
                }, {
                    xtype: 'combo',
                    store: statusStore,
                    name: 'P2',
                    id: 'P2',
                    fieldLabel: '是否已扣庫',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                },
                {
                    xtype: 'combo',
                    store: whnoStore,
                    name: 'P3',
                    id: 'P3',
                    fieldLabel: '庫房',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'textfield',
                name: 'P4',
                id: 'P4',
                margin: '4 4 0 4',
                fieldLabel: '院內碼'
            }, {
                xtype: 'button',
                text: '查詢',
                margin: '4 4 0 70',
                handler: function () {
                    if (!T1Query.getForm().findField('P0').getValue() ||
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">刷條碼時間</span>為必填');
                        return;
                    }

                    T1Load();
                }
            }, {
                xtype: 'button',
                text: '清除',
                margin: '4 4 0 4',
                handler: function () {
                    var f = this.up('form').getForm();

                    f.reset();

                    msglabel('訊息區:');
                }
            }, {
                xtype: 'button',
                text: '庫存歷史查詢',
                margin: '4 4 0 4',
                handler: function () {
                    if (view_all== "Y"){
                        popWinForm('AA0044', '庫存歷史查詢');
                    }
                    else{
                        popWinForm('AB0035', '庫存歷史查詢');
                    }
                }
            }, {
                xtype: 'button',
                text: '庫存現況查詢',
                margin: '4 4 0 4',
                handler: function () {
                    if (view_all == "Y") {
                        popWinForm('AA0129', '庫存現況查詢');
                    }
                    else {
                        popWinForm('AB0096', '庫存現況查詢');
                    }
                }
            }]
        }]
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '匯出', handler: function () {
                    if (!T1Query.getForm().findField('P0').getValue() ||
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">刷條碼時間</span>不可空白');
                        return;
                    }

                    var p = new Array();
                    p.push({ name: 'start_date', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'end_date', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'status', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'wh_no', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'mmcode', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'view_all', value: view_all });
                    PostForm(T1GetExcel, p);
                    //msglabel('匯出完成');
                }
            },
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "刷條碼時間",
            dataIndex: 'CREATE_TIME',
            width: 100
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 80
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 130
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_C',
            width: 130
        }, {
            text: "扣庫前庫存量",
            dataIndex: 'BF_INVQTY',
            width: 90
        }, {
            text: "異動類別",
            dataIndex: 'USE_TYPE',
            width: 70,
        }, {
            text: "扣庫量/繳回量",
            dataIndex: 'USE_QTY',
            width: 100,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 70,
        }, {
            text: "批號效期註記",
            dataIndex: 'WEXP_ID',
            width: 90
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            width: 70,
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            width: 80
        }, {
            text: "盤差種類",
            dataIndex: 'M_TRNID',
            width: 80,
        }, {
            text: "來源代碼",
            dataIndex: 'E_SOURCECODE',
            width: 80
        }, {
            text: "是否已扣庫",
            dataIndex: 'ISUSE',
            width: 100
        }, {
            text: "備註",
            dataIndex: 'SUSE_NOTE',
            width: 100
        }, {
            text: "刷條碼人員",
            dataIndex: 'CREATE_USER',
            width: 110
        }, {
            text: "轉換後數量",
            dataIndex: 'TRATIO',
            width: 80
        }, {
            text: "扣庫倍率",
            dataIndex: 'ACKTIMES',
            width: 70
        }, {
            text: "散裝量",
            dataIndex: 'ADJQTY',
            width: 60
        }, {
            text: "前端掃入的條碼",
            dataIndex: 'SCAN_BARCODE',
            width: 110
        }, {
            text: "序號",
            dataIndex: 'SUSE_SEQ',
            width: 80
        }, {
            header: "",
            flex: 1
        },
        ]
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }
    //#region 呼叫其他網頁程式
    var callableWin = null;
    popWinForm = function (url,caption) {
        var strUrl = url;
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });
            var title = caption;
            callableWin = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }
    //#endregion

    //#region viewport
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }
        ]
    });
    //#endregion

});