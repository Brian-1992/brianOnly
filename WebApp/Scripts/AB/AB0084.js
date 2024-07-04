Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除

    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });



    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
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
        layout: {
            type: 'vbox',
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '查詢日期',
                    name: 'D0',
                    id: 'D0',
                    vtype: 'dateRange',
                    dateRange: { end: 'D2' },
                    labelWidth: 60,
                    width: 160
                }, {
                    xtype: 'timefield',
                    name: 'D1',
                    id: 'D1',
                    reference: 'timeField',
                    format: 'H:i',
                    maxValue: '24:00',
                    increment: 5,
                    width: 100
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelWidth: '10px',
                    name: 'D2',
                    id: 'D2',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'D0' },
                    labelWidth: 7,
                    width: 107
                }, {
                    xtype: 'timefield',
                    name: 'D3',
                    id: 'D3',
                    reference: 'timeField',
                    format: 'H:i',
                    maxValue: '24:00',
                    increment: 5,
                    width: 100
                }, {
                    xtype: 'textfield',
                    fieldLabel: '病歷號',
                    name: 'P0',
                    id: 'P0',
                    labelWidth: 60,
                    width: 160

                }, {
                    xtype: 'button',
                    text: '列印',
                    handler: function () {
                        
                            reportUrl = '/Report/A/AB0084.aspx';
                            showReport();
                        
                    }
                }, {
                    xtype: 'button',
                    text: '列印總表',
                    handler: function () {

                        reportUrl = '/Report/A/AB0084Sum.aspx';
                        showReport();

                    }

                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('D0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?D0=' + T1Query.getForm().findField('D0').rawValue
                + '&D1=' + T1Query.getForm().findField('D1').rawValue
                + '&D2=' + T1Query.getForm().findField('D2').rawValue
                + '&D3=' + T1Query.getForm().findField('D3').rawValue
                + '&P0=' + T1Query.getForm().findField('P0').getValue()
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    var T1Store = Ext.create('WEBAPP.store.MiUnitcodeAA0119', { // 定義於/Scripts/app/store/MiUnitcodeAA0119.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        }],
    });

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
        },
        ]
    });
});