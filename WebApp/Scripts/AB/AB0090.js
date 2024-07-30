Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var T1RecLength = 0;
    var T1LastRec = null;
    var wh_no = '';
    var mmcode1 = '';
    var mmcode2 = '';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1GetExcel = '../../../api/AA0090/Excel';
    

    var st_whno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0090/GetWhnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    wh_kind: '0',
                    wh_grade: '1'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        autoLoad: true
    });

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P3',
        name: 'P3',
        fieldLabel: '藥品代碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 180,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0090/GetMmCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });
    var T1QuryMMCode2 = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P4',
        name: 'P4',
        fieldLabel: '至',
        labelAlign: 'right',
        labelWidth: 10,
        width: 130,
        labelSeparator: '',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0090/GetMmCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    var today = new Date();
    var y = today.getFullYear();
    var m = today.getMonth();
    var d = today.getDate();
    var pym = null;

    var date_10908 = new Date('2020-08-01');
    var date_b = new Date(y, m - 12);
    if (date_b < date_10908) {
        date_b = new Date('2020-08-01');
    }
    var date_e = new Date(y, m - 1);
    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 160;
    var orderdcflagStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { VALUE: '', TEXT: '全部' },
            { VALUE: 'Y', TEXT: 'Y' },
            { VALUE: 'N', TEXT: 'N' },
        ]
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
                    fieldLabel: 'Update',
                    name: 'x',
                    xtype: 'hidden'
                }, 
                {
                    xtype: 'monthfield',
                    fieldLabel: '月份',
                    labelAlign: 'right',
                    name: 'P0',
                    id: 'P0',
                    labelWidth: mLabelWidth,
                    width: 130,
                    padding: '0 4 0 4',
                    value: date_b,
                    readOnly: true
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '至',
                    labelAlign: 'right',
                    labelWidth: 10,
                    name: 'P1',
                    id: 'P1',
                    labelSeparator: '',
                    width: 90,
                    padding: '0 4 0 4',
                    value: date_e,
                    allowBlank: false,
                    fieldCls: 'required',
                    listeners: {
                        change: function (field, nVal, oVal, eOpts) {
                            if (nVal < date_10908) {
                                nVal = new Date('2020-08-01');
                                T1Query.getForm().findField('P1').setValue(new Date('2020-08-01'));
                                Ext.Msg.alert('提醒', '月份不可小於10908');
                            }
                            
                            y = nVal.getFullYear();
                            m = nVal.getMonth();
                            
                            date_b = new Date(y, m - 11)
                            if (date_b < date_10908) {
                                date_b = new Date('2020-08-01');
                            }
                            T1Query.getForm().findField('P0').setValue(date_b);
                        }
                    }
                }, {
                    xtype: 'combo',
                    fieldLabel: '庫別代碼',
                    name: 'P2',
                    id: 'P2',
                    store: st_whno,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    anyMatch: true,
                    allowBlank: false,
                    fieldCls: 'required',
                    padding: '0 4 0 4'
                },
                T1QuryMMCode,
                T1QuryMMCode2,
                {
                    xtype: 'combo',
                    store: orderdcflagStore,
                    name: 'P5',
                    id: 'P5',
                    fieldLabel: '全院停用碼',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: true,
                    typeAhead: true,
                    forceSelection: true,
                    width: 130,
                    labelWidth:70,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    value: ''
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (!T1Query.getForm().findField('P0').getValue() ||
                            !T1Query.getForm().findField('P1').getValue() ||
                            !T1Query.getForm().findField('P2').getValue()) {

                            Ext.Msg.alert('提醒', '<span style="color:red">月份區間</span>與<span style="color:red">庫房代碼</span>為必填');
                            return;
                        }


                        T1Load();
                    },
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        T1Query.getForm().findField('P0').reset();
                        T1Query.getForm().findField('P1').reset();

                        var f = this.up('form').getForm();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                },
                //{
                //    xtype: 'button',
                //    text: '',
                //    id:'transfer',
                //    handler: function () {
                //        Ext.MessageBox.confirm('提示', '是否確定更新' + pym + '資料？', function (btn, text) {
                //            if (btn === 'yes') {
                //                //transferToBcwhpick("");
                //                myMaskFull.show();
                //                updateConsumeMnWh('N');
                //            }
                //        }
                //        );
                //    }
                //}
            ]
        }]

    });

    Ext.define('ExcelModel', {
        extend: 'Ext.data.Model',
        Property: 'MMCODE',
        fields: [
            { name: 'TR_MCODE', cname: '消/進' },
            { name: 'CONTRACNO', cname: '合約碼' },
            { name: 'DRUGPTYCODE1', cname: '主藥理' },
            { name: 'DRUGPTYCODE2', cname: '次藥理' },
            { name: 'INSUSIGNI', cname: '健保' },
            { name: 'MMCODE', cname: '院內代碼' },
            { name: 'MMNAME_E', cname: '藥品名稱' },
            { name: 'E_SCIENTIFICNAME', cname: '主成份' },
            { name: 'E_ORDERUNIT', cname: '計價單位' },
            { name: 'DISC_UPRICE', cname: '進價' },
            { name: 'M_CONTPRICE', cname: '合約價' },
            { name: 'MAMAGERATE', cname: '優惠百分比' },
            { name: 'AVG_PRICE', cname: '移動加權平均價' },
            { name: 'NHI_PRICE', cname: '健保價' },
            { name: 'M_NHIKEY', cname: '健保碼' },
            { name: 'AGEN_NAME', cname: '廠商名稱' },
            { name: 'M_AGENLAB_RMK', cname: '廠牌註記' },
            { name: 'M_AGENLAB', cname: '廠牌' },
            { name: 'AGENTNAME1', cname: '申請商註記' },
            { name: 'AGENTNAME2', cname: '申請商' },
            { name: 'TR_DATE', cname: '年月' }
        ]
    });
    
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'TR_MCODE', type: 'string' },
            { name: 'CONTRACNO', type: 'string' },
            { name: 'DRUGPTYCODE1', type: 'string' },
            { name: 'DRUGPTYCODE2', type: 'string' },
            { name: 'INSUSIGNI', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'E_SCIENTIFICNAME', type: 'string' },
            { name: 'E_ORDERUNIT', type: 'string' },
            { name: 'UPRICE', type: 'string' },
            { name: 'DISC_UPRICE', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'MAMAGERATE', type: 'string' },
            { name: 'AVG_PRICE', type: 'string' },
            { name: 'NHI_PRICE', type: 'string' },
            { name: 'M_NHIKEY', type: 'string' },
            { name: 'AGEN_NAME', type: 'string' },
            { name: 'M_AGENLAB_RMK', type: 'string' },
            { name: 'M_AGENLAB', type: 'string' },
            { name: 'AGENTNAME1', type: 'string' },
            { name: 'AGENTNAME2', type: 'string' },
            { name: 'TR_DATE', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'TR_MCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                start_date = T1Query.getForm().findField('P0').rawValue;
                end_date = T1Query.getForm().findField('P1').rawValue;
                wh_no = T1Query.getForm().findField('P2').getValue();
                mmcode1 = T1Query.getForm().findField('P3').getValue();
                mmcode2 = T1Query.getForm().findField('P4').getValue();
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },load: function (store, records, successful, eOpts) {
                if (records.length > 0) {
                    Ext.getCmp('T1xls').setDisabled(false);
                }
            }

        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            timeout: 180000,
            url: '/api/AB0090/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'T1xls',
                id: 'T1xls',
                text: '匯出', disabled: true, border: 1,
                style: {
                    borderColor: '#0080ff',
                    borderStyle: 'solid'
                },
                handler: function () {
                    if (T1Store.getCount() > 0) {
                        var d1 = start_date;
                        var d2 = end_date;
                        var w = wh_no;
                        var m1 = mmcode1;
                        var m2 = mmcode2;
                        var e = d1 + ',' + d2 + ',' + w + ',' + m1 + ',' + m2;
                        var p = new Array();
                        var filename = wh_no == 'all_phd' ? '全部藥局' : wh_no;
                        p.push({ name: 'FN', value: '前12月每月消耗及進貨_' + filename + '.xls' }); //檔名
                        p.push({ name: 'TS', value: e }); //SQL篩選條件
                        p.push({ name: 'start_date', value: start_date }); //SQL篩選條件
                        p.push({ name: 'end_date', value: end_date }); //SQL篩選條件
                        p.push({ name: 'wh_no', value: wh_no }); //SQL篩選條件
                        p.push({ name: 'mmcode1', value: mmcode1 }); //SQL篩選條件
                        p.push({ name: 'mmcode2', value: mmcode2 }); //SQL篩選條件
                        p.push({ name: 'orderdcflag', value: T1Query.getForm().findField('P5').getValue() }); //SQL篩選條件

                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中，檔案將自行下載，請勿離開畫面或使用其他功能' });
                        myMask.show();
                        PostForm('/api/AB0090/GetExcel', p);
                        setTimeout(function () {
                            myMask.hide();
                        }, 3000);
                        
                    }
                    else
                        Ext.Msg.alert('訊息', '沒有資料');

                }
            }
        ]
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
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "消/進",
                dataIndex: 'TR_MCODE',
                style: 'text-align:left',
                align: 'left',
                width: 60
            }, {
                text: "合約碼",
                dataIndex: 'CONTRACNO',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "主藥理",
                dataIndex: 'DRUGPTYCODE1',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "次藥理",
                dataIndex: 'DRUGPTYCODE2',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "健保",
                dataIndex: 'INSUSIGNI',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "院內代碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "藥品名稱",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "主成份",
                dataIndex: 'E_SCIENTIFICNAME',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "計價單位",
                dataIndex: 'E_ORDERUNIT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "進價",
                dataIndex: 'DISC_UPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "合約價",
                dataIndex: 'M_CONTPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "優惠百分比",
                dataIndex: 'MAMAGERATE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "移動加權平均價",
                dataIndex: 'AVG_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "健保價",
                dataIndex: 'NHI_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "健保碼",
                dataIndex: 'M_NHIKEY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAME',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "廠牌註記",
                dataIndex: 'M_AGENLAB_RMK',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "廠牌",
                dataIndex: 'M_AGENLAB',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "申請商註記",
                dataIndex: 'AGENTNAME1',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "申請商",
                dataIndex: 'AGENTNAME2',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
    });

    function getPym() {
        Ext.Ajax.request({
            url: '/api/AB0090/GetPym',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    pym = data.msg;
                    Ext.getCmp('transfer').setText('更新' + pym + '資料');
                } 
            },
            failure: function (response, options) {

            }
        });
    }

    function updateConsumeMnWh(isInit) {
        Ext.Ajax.request({
            url: '/api/AB0090/UpdateConsumeMnWh',
            method: reqVal_p,
            params: { isInit: isInit },
            timeout:800000,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                myMaskFull.hide();
                if (data.success) {
                    if (isInit == 'Y') {
                        console.log(data.msg);
                    } else {
                        msglabel('資料更新完成，請重新查詢')
                    }
                } else {
                    if (isInit == 'Y') {
                        console.log('資料更新失敗');
                    } else {
                        Ext.Msg.alert('錯誤', data.msg);
                    }
                }
            },
            failure: function (response, options) {
                myMaskFull.hide();
            }
        });
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
        },
        ]
    });



   // var myMaskFull = new Ext.LoadMask(viewport, { msg: '資料更新中...' });
   // myMaskFull.hide();
    // 取得上月
    getPym();
    // 每次進入都更呼叫stored procedure更新 2021-08-29取消
   // myMaskFull.show();
    //updateConsumeMnWh('N');
});