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
    var St_YEAR = '../../../api/FA0074/GetSt_YEAR';


    //資料年
    var st_YEAR = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });


    function setComboData() {

        //資料年
        Ext.Ajax.request({
            url: St_YEAR,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_ym = data.etts;


                    if (tb_ym.length > 0) {
                        for (var i = 0; i < tb_ym.length; i++) {
                            st_YEAR.add({ VALUE: tb_ym[i].VALUE, COMBITEM: tb_ym[i].COMBITEM });
                            T1Query.getForm().findField('P0').setValue(tb_ym[i].VALUE);
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });


    }
    setComboData();

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
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'combo',
                fieldLabel: '資料年',
                name: 'P0',
                id: 'P0',
                store: st_YEAR,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                multiSelect: true,
                queryMode: 'local',
                labelWidth: 70,
                width: 220,
            }, {
                xtype: 'button',
                text: '查詢',
                handler: T1Load
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    msglabel('訊息區:');
                }
            }
            ]
        }]
    });


    var T1Store = Ext.create('WEBAPP.store.FA0074', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('print').setDisabled(false);
                } else {
                    Ext.getCmp('print').setDisabled(true);
                }
            }
        }
    });

    function T1Load() {
        if (T1Query.getForm().findField('P0').getValue() != "") {
            T1Tool.moveFirst();
            msglabel('訊息區:');
            //viewport.down('#form').collapse();
        } else {
            Ext.Msg.alert("訊息", "資料年不能空白");
        }

    }

    // toolbar 
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '列印',
                id: 'print',
                name: 'print',
                handler: function () {
                    if (T1Store.getCount() > 0) {
                        reportUrl = '/Report/F/FA0074.aspx';
                        showReport();
                    }
                    else
                        Ext.Msg.alert('訊息', '沒有資料');
                }
            }
        ]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?type=' + "FA0074"
                + '&year=' + T1Query.getForm().findField('P0').getValue()
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
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                text: "分類",
                dataIndex: 'MAT_CLASS',
                style: 'text-align:left',
                align: 'left',
                width: 120
            }, {
                text: "統計項目",
                dataIndex: 'ITEM',
                style: 'text-align:left',
                align: 'left',
                width: 90
            }, {
                text: "一月",
                dataIndex: 'M01',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "二月",
                dataIndex: 'M02',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "三月",
                dataIndex: 'M03',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "四月",
                dataIndex: 'M04',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "五月",
                dataIndex: 'M05',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "六月",
                dataIndex: 'M06',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "七月",
                dataIndex: 'M07',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "八月",
                dataIndex: 'M08',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "九月",
                dataIndex: 'M09',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "十月",
                dataIndex: 'M10',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "十一月",
                dataIndex: 'M11',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "十二月",
                dataIndex: 'M12',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "年平均值",
                dataIndex: 'MAVG',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "年存貨週轉率",
                dataIndex: 'AVGINV_RATIO',
                style: 'text-align:left',
                align: 'right',
                width: 120
            }, {
                text: "年平均耗用天數",
                dataIndex: 'AVGDAYS',
                style: 'text-align:left',
                align: 'right',
                width: 120
            }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
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
        }
        ]
    });
    Ext.getCmp('print').setDisabled(true);
});