Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/FA0030/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "單位總材積轉檔";
    var T1GetDept = '/api/FA0030/GetDept';
    var T1GetExcel = '/api/FA0030/Excel';
    var reportUrl = '/Report/F/FA0030.aspx';

    var T1Rec = 0;
    var T1LastRec = null;
    var DATA_YM;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'monthfield',
                    fieldLabel: '申領年月',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    padding: '0 4 0 4',
                    allowBlank: false,
                    fieldCls: 'required',
                    value: new Date(),
                    labelWidth: 60,
                    width: 170,
                    blankText: '必須輸入要查詢的申領年月資料'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        if (T1Query.getForm().isValid()) {
                            T1Load();
                        }
                        else {
                            T1Store.removeAll();
                            if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                                Ext.Msg.alert('提醒', '必須輸入要查詢的<span style=\'color:red\'>申領年月</span>資料');
                                msglabel(" 必須輸入要查詢的<span style=\'color:red\'>申領年月</span>資料");
                            }
                            else {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>申領年月</span>資料不正確');
                                msglabel(" <span style=\'color:red\'>申領年月</span>資料不正確");
                            }
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.FA0030', { // 定義於/Scripts/app/store/FA0030.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                DATA_YM = T1Query.getForm().findField('P0').rawValue;
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '單位總材積轉檔.xls' });
                    p.push({ name: 'p0', value: DATA_YM });
                    PostForm(T1GetExcel, p);
                    msglabel('匯出完成');
                }
            },
            {
                itemId: 'print', text: '列印', handler: function () {
                    showReport();
                }
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer', // 0
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        },
        {
            text: "成本碼",
            dataIndex: 'APPDEPT',
            width: 60
        }, {
            text: "單位名稱",
            dataIndex: 'APPDEPT_NAME',
            width: 100
        }, {
            text: "總材積",
            dataIndex: 'SUM_APV_VOLUME',
            width: 100,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }, {
            header: "",
            flex: 1
        }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#print').setDisabled(T1Store.getCount() === 0);
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec) {
                    msglabel("");
                }
            }
        }
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?DATA_YM=' + DATA_YM
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }

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
        }]
    });

    T1Query.getForm().findField('P0').focus(); //讓游標停在P0這一格
});
