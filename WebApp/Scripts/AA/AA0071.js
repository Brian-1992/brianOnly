Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.tip.QuickTipManager.init();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Exp = '../../../api/AA0071/Excel';
    //var T1Name = "中央庫房(衛材)調帳明細表";

    var viewModel = Ext.create('WEBAPP.store.AA.AA0122VM');
    var MatClassStore = viewModel.getStore('MAT_CLASS');
    //function MatClassLoad() {
    //    MatClassStore.load({
    //        params: {
    //            start: 0
    //        }
    //    });
    //}

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P2',
        name: 'P2',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 55,
        width: 275,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0071/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p2: T1Query.getForm().findField('P1').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 95
        },
        border: false,
        items: [
        {
            xtype: 'datefield',
            fieldLabel: '申請調帳日期區間',
            name: 'D0',
            id: 'D0',
            vtype: 'dateRange',
            dateRange: { end: 'D1' },
            value: getDefaultValue("D0"),
            labelWidth: 120,
            width: 220
        }, {
            xtype: 'datefield',
            fieldLabel: '至',
            labelWidth: '10px',
            name: 'D1',
            id: 'D1',
            labelSeparator: '',
            vtype: 'dateRange',
            dateRange: { begin: 'D0' },
            value: getDefaultValue("D1"),
            labelWidth: 7,
            width: 107
        }, {
            xtype: 'combo',
            fieldLabel: '物料分類',
            name: 'P1',
            id: 'P1',
            store: MatClassStore,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            multiSelect: true,
            queryMode: 'local',
            triggerAction: 'all',
            labelWidth: 65,
            width: 185
        },
                T1QuryMMCode,
        {
            xtype: 'button',
            text: '查詢',
            handler: T1Load
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P1').focus();
            }
        }]
    });

    function getDefaultValue(isEndDate) {
        var yyyy = 0;
        var m = 0;
        if (isEndDate == "D1") {
            yyyy = new Date().getFullYear() - 1911;
            m = new Date().getMonth() + 1;
        } else if (isEndDate == "D0") {    //減6個月
            var date = new Date();
            date.setMonth(date.getMonth() - 5);

            yyyy = date.getFullYear() - 1911;
            m = date.getMonth();
            if (m == 0) {   //因為從目前六月算起，的前六月是12月，但它跑出來是0
                yyyy = yyyy - 1;
                m = 12;
            }
        }

        var d = 0;
        d = new Date().getDate();

        var mm = m > 10 ? m.toString() : "0" + m.toString();
        var dd = d > 10 ? d.toString() : "0" + d.toString();

        return yyyy.toString() + mm + dd;
    }

    var T1Store = Ext.create('WEBAPP.store.AA.AA0071', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    d0: T1Query.getForm().findField('D0').getRawValue(),
                    d1: T1Query.getForm().findField('D1').getRawValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
    }
    //T1Load();

    var mmCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0041/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        //getDefaultParams: function () {
            //return { INID: T1Form.getForm().findField('INID').getValue() };
        //},
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                alert(r.get('MAT_CLASS'));
            }
        }
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '列印', handler: function () {
                    //if (T1Store.getCount() > 0) {
                    showReport();
                    //}
                    //else
                    // Ext.Msg.alert('訊息', '請先建立明細資料');
                }
            }, {
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'FN', value: '中央庫房(衛材)調帳明細表匯出.xls' }); //檔名
                            p.push({ name: 'd0', value: T1Query.getForm().findField('D0').getRawValue() }); //SQL篩選條件
                            p.push({ name: 'd1', value: T1Query.getForm().findField('D1').getRawValue() }); //SQL篩選條件
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件
                            p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件
                            PostForm(T1Exp, p);
                            msglabel('訊息區:匯出完成');
                        }
                    });
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
                html: '<iframe src="/Report/A/AA0071.aspx?d0=' + T1Query.getForm().findField('D0').getRawValue()
                    + '&d1=' + T1Query.getForm().findField('D1').getRawValue()
                    + '&p1=' + T1Query.getForm().findField('P1').getValue()
                    + '&p2=' + T1Query.getForm().findField('P2').getValue()
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

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
            text: "單據號碼",
            dataIndex: 'DOCNO',
            width: 110
        }, {
            text: "申請日期",
            dataIndex: 'APPTIME',
            width: 80
        }, {
            text: "異動代碼",
            dataIndex: 'DOCTYPE_N',
            width: 110
        }, {
            text: "物料分類",
            dataIndex: 'MAT_CLASS_N',
            width: 100
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "品名",
            dataIndex: 'MMNAME',
            width: 250
        }, {
            text: "調帳數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            style: 'text-align:left',
            align: 'left',
            width: 60
        }, {
            text: "合約單價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 80
        
        }, {
            sortable: false,
            flex: 1
        }]
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
    T1Query.getForm().findField('P1').focus();
});
